using CSE.Automation.Model;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

#pragma warning disable CA1031 // Do not catch general exception types

namespace CSE.Automation.Graph
{
    public class ServicePrincipalGraphHelper : GraphHelperBase<ServicePrincipal>
    {
        public ServicePrincipalGraphHelper(GraphHelperSettings settings, ILogger<ServicePrincipalGraphHelper> logger) : base(settings, logger) { }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Console.WriteLine will be changed to logs")]
        public override async Task<(string, IEnumerable<ServicePrincipal>)> GetDeltaGraphObjects(string selectFields, ProcessorConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
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
                .Select(selectFields)
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
            _logger.LogDebug($"`tDiscovered {servicePrincipalCollectionPage.CurrentPage.Count} Service Principals");

            while (servicePrincipalCollectionPage.NextPageRequest != null)
            {
                servicePrincipalCollectionPage = await servicePrincipalCollectionPage.NextPageRequest.GetAsync().ConfigureAwait(false);
                servicePrincipalSeedList.AddRange(servicePrincipalCollectionPage.CurrentPage);
                _logger.LogDebug($"`tDiscovered {servicePrincipalCollectionPage.CurrentPage.Count} Service Principals");
            }

            _logger.LogInformation($"Discovered {servicePrincipalSeedList.Count} delta objects.");

            servicePrincipalCollectionPage.AdditionalData.TryGetValue("@odata.deltaLink", out object updatedDeltaLink);

            return (updatedDeltaLink?.ToString(), servicePrincipalSeedList);
        }
    }
}
