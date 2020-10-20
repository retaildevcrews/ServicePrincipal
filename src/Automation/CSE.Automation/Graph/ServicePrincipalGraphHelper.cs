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

            var servicePrincipalSeedList = new List<ServicePrincipal>();

            if (config.RunState == RunState.Seedonly ||
                config.RunState == RunState.SeedAndRun ||
                String.IsNullOrEmpty(config.DeltaLink))
            {
                _logger.LogInformation("Seeding Service Principal objects from Graph...");

                servicePrincipalCollectionPage = await graphClient.ServicePrincipals
                .Delta()
                .Request()
                .Expand("Owners")
                .Top(500)
                //.Request(new[] { new QueryOption("$top", "500") })
                //.Select(selectFields)
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

            servicePrincipalSeedList.AddRange(servicePrincipalCollectionPage.CurrentPage);
            _logger.LogDebug($"\tDiscovered {servicePrincipalCollectionPage.CurrentPage.Count} Service Principals");

            while (servicePrincipalCollectionPage.NextPageRequest != null)
            {
                servicePrincipalCollectionPage = await servicePrincipalCollectionPage.NextPageRequest.GetAsync().ConfigureAwait(false);
                servicePrincipalSeedList.AddRange(servicePrincipalCollectionPage.CurrentPage);
                _logger.LogDebug($"\tDiscovered {servicePrincipalCollectionPage.CurrentPage.Count} Service Principals");
                if (_settings.ScanLimit.HasValue && servicePrincipalSeedList.Count > _settings.ScanLimit) break;
            }

            _logger.LogInformation($"Discovered {servicePrincipalSeedList.Count} delta objects.");

            var foundItem = servicePrincipalSeedList.FirstOrDefault(x => string.Equals(x.Id, "ea016769-ea50-4b6e-a691-23996eaf378a", StringComparison.OrdinalIgnoreCase));
            var ownedItems = servicePrincipalSeedList.Where(x => x.Owners != null).ToList();

            servicePrincipalCollectionPage.AdditionalData.TryGetValue("@odata.deltaLink", out object updatedDeltaLink);

            return (updatedDeltaLink?.ToString(), servicePrincipalSeedList);
        }
    }
}
