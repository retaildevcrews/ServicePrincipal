using CSE.Automation.Model;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using CSE.Automation.Interfaces;

#pragma warning disable CA1031 // Do not catch general exception types

namespace CSE.Automation.Graph
{
    internal class ServicePrincipalGraphHelper : GraphHelperBase<ServicePrincipal>
    {


        public ServicePrincipalGraphHelper(GraphHelperSettings settings, IAuditService auditService, ILogger<ServicePrincipalGraphHelper> logger) : base(settings, auditService, logger) { }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Console.WriteLine will be changed to logs")]
        public override async Task<(GraphOperationMetrics metrics, IEnumerable<ServicePrincipal> data)> GetDeltaGraphObjects(ActivityContext context, ProcessorConfiguration config, string selectFields = null)
        {
            var metrics = new GraphOperationMetrics();

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
                metrics.Name = "Full Seed";

                servicePrincipalCollectionPage = await GraphClient.ServicePrincipals
                .Delta()
                .Request()
                //.Select(selectFields)
                .Top(500)
                .GetAsync()
                .ConfigureAwait(false);
            }
            else
            {
                metrics.Name = "Delta Discovery";

                _logger.LogInformation("Fetching Service Principal Delta objects from Graph...");

                servicePrincipalCollectionPage = new ServicePrincipalDeltaCollectionPage();
                servicePrincipalCollectionPage.InitializeNextPageRequest(GraphClient, config.DeltaLink);
                servicePrincipalCollectionPage = await servicePrincipalCollectionPage.NextPageRequest.GetAsync().ConfigureAwait(false);
            }

            servicePrincipalList.AddRange(servicePrincipalCollectionPage.CurrentPage);

            _logger.LogDebug($"\tDiscovered {servicePrincipalCollectionPage.CurrentPage.Count} Service Principals");

            metrics.Considered = servicePrincipalList.Count;
            while (servicePrincipalCollectionPage.NextPageRequest != null)
            {
                servicePrincipalCollectionPage = await servicePrincipalCollectionPage.NextPageRequest.Top(500).GetAsync().ConfigureAwait(false);

                var pageList = servicePrincipalCollectionPage.CurrentPage;
                var count = pageList.Count;

                metrics.Considered += count;

                _logger.LogDebug($"\tDiscovered {count} Service Principals");

                if (IsSeedRun(config))
                {
                    _logger.LogInformation($"Trimming removed service principals for seed run.");
                    pageList = pageList.Where(x => x.AdditionalData.Keys.Contains("@removed") == false).ToList();
                    pageList.ToList().ForEach(sp => _auditService.PutIgnore(
                        context: context,
                        code: AuditCode.Ignore_ServicePrincipalDeleted,
                        objectId: sp.Id,
                        attributeName: "AdditionalData",
                        existingAttributeValue: "@removed"));

                    var removedCount = count - pageList.Count;

                    _logger.LogInformation($"\tTrimmed {removedCount} service principals.");
                    metrics.Removed += removedCount;
                }

                servicePrincipalList.AddRange(pageList);
            }

            _logger.LogInformation($"Discovered {servicePrincipalList.Count} delta objects.");
            metrics.Found = servicePrincipalList.Count;

            servicePrincipalCollectionPage.AdditionalData.TryGetValue("@odata.deltaLink", out object updatedDeltaLink);

            metrics.AdditionalData = updatedDeltaLink?.ToString();

            return (metrics, servicePrincipalList);
        }

        public async override Task<ServicePrincipal> GetGraphObject(string id)
        {
            var entity = await GraphClient.ServicePrincipals[id]
                .Request()
                .Expand("Owners")
                .GetAsync()
                .ConfigureAwait(false);

            return entity;
        }

        //public async override Task<IEnumerable<ServicePrincipal>> GetGraphObjects(IEnumerable<QueryOption> queryOptions)
        //{
        //    var entityList = await graphClient.ServicePrincipals
        //                        .Request(queryOptions)
        //                        .GetAsync()
        //                        .ConfigureAwait(false);
        //    return entityList;
        //}
        public async override Task PatchGraphObject(ServicePrincipal servicePrincipal)
        {
            // API call uses a PATCH so only include properties to change
            await GraphClient.ServicePrincipals[servicePrincipal.Id]
                    .Request()
                    .UpdateAsync(servicePrincipal)
                    .ConfigureAwait(false);
        }

        #region HELPER

        private static bool IsSeedRun(ProcessorConfiguration config)
        {
            return
                config.RunState == RunState.Seedonly ||
                config.RunState == RunState.SeedAndRun ||
                String.IsNullOrEmpty(config.DeltaLink);
        }
        #endregion
    }
}
