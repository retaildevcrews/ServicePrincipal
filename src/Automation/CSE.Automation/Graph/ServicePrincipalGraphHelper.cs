// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

#pragma warning disable CA1031 // Do not catch general exception types

namespace CSE.Automation.Graph
{
    internal class TestServicePrincipalGraphHelper : ServicePrincipalGraphHelper
    {
        private string displayNamePatternFilter;

        public TestServicePrincipalGraphHelper(GraphHelperSettings settings, IAuditService auditService, IGraphServiceClient graphClient, string displayNamePatternFilter, ILogger<ServicePrincipalGraphHelper> logger)
            : base(settings, auditService, graphClient, logger)
        {
            this.displayNamePatternFilter = displayNamePatternFilter;
        }

        protected virtual IServicePrincipalDeltaRequest GetGraphSeedRequest()
        {
            return GraphClient
                .ServicePrincipals
                .Delta()
                .Request()
                .Filter(GetFilterString(displayNamePatternFilter));
        }

        private string GetFilterString(string displayNamePatternFilter)
        {
            List<ServicePrincipal> servicePrincipalList = new List<ServicePrincipal>();

            var servicePrincipalsPage = GraphClient.ServicePrincipals
                .Request()
                .Filter($"startswith(displayName,'{displayNamePatternFilter}')")
                .GetAsync().Result;

            servicePrincipalList.AddRange(servicePrincipalsPage.CurrentPage);

            // NOTE: The number of ids you can specify is limited by the maximum URL length
            // Successfully tested a request like this Delta().Request().Filter("filter string for up to 200 SPs") with 200 SP IDs so 100 should not be a problem.
            while (servicePrincipalsPage.NextPageRequest != null && servicePrincipalList.Count < 100)
            {
                servicePrincipalsPage = servicePrincipalsPage.NextPageRequest.GetAsync().Result;
                servicePrincipalList.AddRange(servicePrincipalsPage.CurrentPage);
            }

            string filterTemplate = string.Empty;

            foreach (var spObject in servicePrincipalList)
            {
                if (string.IsNullOrEmpty(filterTemplate))
                {
                    filterTemplate = $"id eq '{spObject.Id}'";
                }
                else
                {
                    filterTemplate += $" or id eq '{spObject.Id}'";
                }
            }

            return filterTemplate;
        }
    }

    internal class ServicePrincipalGraphHelper : GraphHelperBase<ServicePrincipal>
    {
        public ServicePrincipalGraphHelper(GraphHelperSettings settings, IAuditService auditService, IGraphServiceClient graphClient, ILogger<ServicePrincipalGraphHelper> logger)
                : base(settings, auditService, graphClient, logger)
        {
        }

        protected virtual IServicePrincipalDeltaRequest GetGraphSeedRequest()
        {
            return GraphClient
                .ServicePrincipals
                .Delta()
                .Request();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Console.WriteLine will be changed to logs")]
        public override async Task<(GraphOperationMetrics metrics, IEnumerable<ServicePrincipal> data)> GetDeltaGraphObjects(ActivityContext context, ProcessorConfiguration config, string displayNamePatternFilter = null, string selectFields = null)
        {
            GraphOperationMetrics metrics = new GraphOperationMetrics();

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (string.IsNullOrWhiteSpace(selectFields))
            {
                selectFields = string.Join(',', config.SelectFields);
            }

            IServicePrincipalDeltaCollectionPage collectionPage;
            var servicePrincipalList = new List<ServicePrincipal>();

            // Build first page of directory elements
            if (IsSeedRun(config))
            {
                logger.LogInformation("Seeding Service Principal objects from Graph...");
                metrics.Name = "Full Seed";

                collectionPage = await GetGraphSeedRequest()
                .GetAsync()
                .ConfigureAwait(false);
            }
            else
            {
                metrics.Name = "Delta Discovery";

                logger.LogInformation("Fetching Service Principal Delta objects from Graph...");

                collectionPage = new ServicePrincipalDeltaCollectionPage();
                collectionPage.InitializeNextPageRequest(GraphClient, config.DeltaLink);
                collectionPage = await collectionPage.NextPageRequest.GetAsync().ConfigureAwait(false);
            }

            servicePrincipalList.AddRange(PruneRemovedOnFirstRun(context, collectionPage, metrics, config));

            while (collectionPage.NextPageRequest != null)
            {
                collectionPage = await collectionPage.NextPageRequest.GetAsync().ConfigureAwait(false);

                servicePrincipalList.AddRange(PruneRemovedOnFirstRun(context, collectionPage, metrics, config));
            }

            logger.LogInformation($"Discovered {servicePrincipalList.Count} delta objects.");
            metrics.Found = servicePrincipalList.Count;

            collectionPage.AdditionalData.TryGetValue("@odata.deltaLink", out object updatedDeltaLink);

            metrics.AdditionalData = updatedDeltaLink?.ToString();

            return (metrics, servicePrincipalList);
        }

        private IList<ServicePrincipal> PruneRemovedOnFirstRun(ActivityContext context, IServicePrincipalDeltaCollectionPage collectionPage, GraphOperationMetrics metrics, ProcessorConfiguration config)
        {
            IList<ServicePrincipal> pageList = collectionPage.CurrentPage ?? new List<ServicePrincipal>();
            var count = pageList.Count;

            metrics.Considered += count;

            logger.LogDebug($"\tDiscovered {count} Service Principals");

            if (IsSeedRun(config))
            {
                // Build secondary list of elements that were removed from the directory
                List<ServicePrincipal> removedList = pageList.Where(x => x.AdditionalData.Keys.Contains("@removed")).ToList();

                // Report Audit Ignore messages for all the elements that were already removed from the directory
                removedList.ToList().ForEach(sp => auditService.PutIgnore(
                    context: context,
                    code: AuditCode.Ignore_ServicePrincipalDeleted,
                    objectId: sp.Id,
                    attributeName: "AdditionalData",
                    existingAttributeValue: "@removed"));
                logger.LogInformation($"\tTrimmed {removedList.Count} ServicePrincipals.");
                metrics.Removed += removedList.Count;

                // Filter list to include only those elements that are currently active
                pageList = pageList.Where(x => x.AdditionalData.Keys.Contains("@removed") == false).ToList();
            }

            return pageList;
        }

        public async override Task<ServicePrincipal> GetGraphObjectWithOwners(string id)
        {
            var entity = await GraphClient.ServicePrincipals[id]
                .Request()
                .Expand("Owners")
                .GetAsync()
                .ConfigureAwait(false);

            return entity;
        }

        public async override Task PatchGraphObject(ServicePrincipal servicePrincipal)
        {
            // API call uses a PATCH so only include properties to change
            await GraphClient.ServicePrincipals[servicePrincipal.Id]
                    .Request()
                    .UpdateAsync(servicePrincipal)
                    .ConfigureAwait(false);
        }

        private static bool IsSeedRun(ProcessorConfiguration config)
        {
            return
                config.RunState == RunState.Seed ||
                string.IsNullOrEmpty(config.DeltaLink);
        }
    }
}
