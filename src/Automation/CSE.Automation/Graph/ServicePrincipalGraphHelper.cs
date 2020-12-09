﻿// Copyright (c) Microsoft Corporation. All rights reserved.
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

        /// <summary>
        /// Get Delta Graph Objects
        /// </summary>
        /// <param name="context">The Activity Context</param>
        /// <param name="config">The Processor Configuration </param>
        /// <returns>Tasks with Metrics and Service Principal collection</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Console.WriteLine will be changed to logs")]
        public override async Task<(GraphOperationMetrics metrics, IEnumerable<ServicePrincipal> data)> GetDeltaGraphObjects(ActivityContext context, ProcessorConfiguration config)
        {
            GraphOperationMetrics metrics = new GraphOperationMetrics();

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
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

        /// <summary>
        /// Gets Service Principal Graph Object With Owners
        /// </summary>
        /// <param name="id">The Service Principal Oject Id</param>
        /// <returns>Tasks with Service Principal </returns>
        public async override Task<ServicePrincipal> GetGraphObjectWithOwners(string id)
        {
            var entity = await GraphClient.ServicePrincipals[id]
                .Request()
                .Expand("Owners")
                .GetAsync()
                .ConfigureAwait(false);

            return entity;
        }

        /// <summary>
        /// Updates Service Principal Object
        /// </summary>
        /// <param name="servicePrincipal">The Service Principal</param>
        /// <returns>awaitable Task</returns>
        public async override Task PatchGraphObject(ServicePrincipal servicePrincipal)
        {
            // API call uses a PATCH so only include properties to change
            await GraphClient.ServicePrincipals[servicePrincipal.Id]
                    .Request()
                    .UpdateAsync(servicePrincipal)
                    .ConfigureAwait(false);
        }

        protected virtual IServicePrincipalDeltaRequest GetGraphSeedRequest()
        {
            return GraphClient
                .ServicePrincipals
                .Delta()
                .Request();
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

        private static bool IsSeedRun(ProcessorConfiguration config)
        {
            return
                config.RunState == RunState.Seed ||
                string.IsNullOrEmpty(config.DeltaLink);
        }
    }
}
