using CSE.Automation.Model;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;

#pragma warning disable CA1031 // Do not catch general exception types

namespace CSE.Automation.Graph
{
    public class ServicePrincipalGraphHelper : GraphHelperBase<ServicePrincipal>
    {
        public ServicePrincipalGraphHelper(GraphHelperSettings settings, ILogger<ServicePrincipalGraphHelper> logger) : base(settings, logger) { }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Console.WriteLine will be changed to logs")]
        public override async Task<(string, IEnumerable<ServicePrincipal>)> GetDeltaGraphObjects(ProcessorConfiguration config, string selectFields=null)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (string.IsNullOrWhiteSpace(selectFields))
            {
                selectFields = string.Join(',', config.SelectFields);
            }

            IServicePrincipalDeltaCollectionPage servicePrincipalCollectionPage;
            var servicePrincipalList = new List<ServicePrincipal>();

            if (IsSeedRun(config))
            {
                _logger.LogInformation("Seeding Service Principal objects from Graph...");

                servicePrincipalCollectionPage = await graphClient.ServicePrincipals
                .Delta()
                .Request()
                .Top(500)
                .GetAsync()
                .ConfigureAwait(false);
            }
            else
            {
                _logger.LogInformation("Fetching Service Principal Delta objects from Graph...");

                servicePrincipalCollectionPage = new ServicePrincipalDeltaCollectionPage();
                servicePrincipalCollectionPage.InitializeNextPageRequest(graphClient, config.DeltaLink);
                servicePrincipalCollectionPage = await servicePrincipalCollectionPage.NextPageRequest.GetAsync().ConfigureAwait(false);
            }

            servicePrincipalList.AddRange(servicePrincipalCollectionPage.CurrentPage);
            _logger.LogDebug($"\tDiscovered {servicePrincipalCollectionPage.CurrentPage.Count} Service Principals");

            var totalRetrieved = servicePrincipalList.Count;
            while (servicePrincipalCollectionPage.NextPageRequest != null)
            {
                servicePrincipalCollectionPage = await servicePrincipalCollectionPage.NextPageRequest.Top(500).GetAsync().ConfigureAwait(false);
                
                var pageList = servicePrincipalCollectionPage.CurrentPage;
                var count = pageList.Count;
                totalRetrieved += count;
                _logger.LogDebug($"\tDiscovered {count} Service Principals");
                if (IsSeedRun(config))
                {
                    _logger.LogInformation($"Trimming removed service principals for seed run.");
                    pageList = pageList.Where(x => x.AdditionalData.Keys.Contains("@removed") == false).ToList();
                    _logger.LogInformation($"\tTrimmed {count-pageList.Count} service principals.");
                }

                servicePrincipalList.AddRange(pageList);
                //if (_settings.ScanLimit.HasValue && totalRetrieved > _settings.ScanLimit) break;
            }

            _logger.LogInformation($"Discovered {servicePrincipalList.Count} delta objects.");

            var foundItem = servicePrincipalList.FirstOrDefault(x => string.Equals(x.Id, "ea016769-ea50-4b6e-a691-23996eaf378a", StringComparison.OrdinalIgnoreCase));
            //var removedItems = servicePrincipalSeedList.Where(x => x.AdditionalData.Keys.Contains("@removed")).ToList();

            servicePrincipalCollectionPage.AdditionalData.TryGetValue("@odata.deltaLink", out object updatedDeltaLink);


            //return (updatedDeltaLink?.ToString(), new[] { foundItem } );
            return (updatedDeltaLink?.ToString(), servicePrincipalList);
        }

        public async override Task<ServicePrincipal> GetGraphObject(ProcessorConfiguration config, string id)
        {
            var entity = await graphClient.ServicePrincipals[id]
                .Request()
                .Expand("Owners")
                .GetAsync()
                .ConfigureAwait(false);

            return entity;
        }

        private static bool IsSeedRun(ProcessorConfiguration config)
        {
            return
                config.RunState == RunState.Seedonly ||
                config.RunState == RunState.SeedAndRun ||
                String.IsNullOrEmpty(config.DeltaLink);
        }
    }
}
