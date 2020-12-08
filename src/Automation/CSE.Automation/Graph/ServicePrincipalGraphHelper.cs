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
    internal class ServicePrincipalGraphHelper : GraphHelperBase<ServicePrincipal>
    {
        public ServicePrincipalGraphHelper(GraphHelperSettings settings, IAuditService auditService, IGraphServiceClient graphClient, ILogger<ServicePrincipalGraphHelper> logger)
                : base(settings, auditService, graphClient, logger)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Console.WriteLine will be changed to logs")]
        public override async Task<(GraphOperationMetrics metrics, IEnumerable<ServicePrincipal> data)> GetDeltaGraphObjects(ActivityContext context, ProcessorConfiguration config, string displayNamePatternFilter = null, string selectFields = null)
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
                logger.LogInformation("Seeding Service Principal objects from Graph...");
                metrics.Name = "Full Seed";

                string filterString = string.IsNullOrEmpty(displayNamePatternFilter) ? string.Empty : GetFilterString(displayNamePatternFilter);

                servicePrincipalCollectionPage = await GraphClient.ServicePrincipals
                .Delta()
                .Request()
                .Filter(filterString)
                .GetAsync()
                .ConfigureAwait(false);
            }
            else
            {
                metrics.Name = "Delta Discovery";

                logger.LogInformation("Fetching Service Principal Delta objects from Graph...");

                servicePrincipalCollectionPage = new ServicePrincipalDeltaCollectionPage();
                servicePrincipalCollectionPage.InitializeNextPageRequest(GraphClient, config.DeltaLink);
                servicePrincipalCollectionPage = await servicePrincipalCollectionPage.NextPageRequest.GetAsync().ConfigureAwait(false);
            }

            Action processCurrentPage = () =>
            {
                var pageList = servicePrincipalCollectionPage.CurrentPage;
                var count = pageList.Count;

                metrics.Considered += count;

                logger.LogDebug($"\tDiscovered {count} Service Principals");

                if (IsSeedRun(config))
                {
                    pageList = pageList.Where(x => x.AdditionalData.Keys.Contains("@removed") == false).ToList();
                    pageList.ToList().ForEach(sp => auditService.PutIgnore(
                        context: context,
                        code: AuditCode.Ignore_ServicePrincipalDeleted,
                        objectId: sp.Id,
                        attributeName: "AdditionalData",
                        existingAttributeValue: "@removed"));

                    var removedCount = count - pageList.Count;

                    logger.LogInformation($"\tTrimmed {removedCount} ServicePrincipals.");
                    metrics.Removed += removedCount;
                }

                servicePrincipalList.AddRange(pageList);
            };

            processCurrentPage();

            while (servicePrincipalCollectionPage.NextPageRequest != null)
            {
                servicePrincipalCollectionPage = await servicePrincipalCollectionPage.NextPageRequest.GetAsync().ConfigureAwait(false);

                processCurrentPage();
            }
            logger.LogInformation($"Discovered {servicePrincipalList.Count} delta objects.");
            metrics.Found = servicePrincipalList.Count;

            servicePrincipalCollectionPage.AdditionalData.TryGetValue("@odata.deltaLink", out object updatedDeltaLink);

            metrics.AdditionalData = updatedDeltaLink?.ToString();

            return (metrics, servicePrincipalList);
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

        private static bool IsSeedRun(ProcessorConfiguration config)
        {
            return
                config.RunState == RunState.Seedonly ||
                config.RunState == RunState.SeedAndRun ||
                string.IsNullOrEmpty(config.DeltaLink);
        }
    }
}
