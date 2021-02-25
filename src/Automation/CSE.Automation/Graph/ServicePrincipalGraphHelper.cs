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
    internal class ServicePrincipalGraphHelper : GraphHelperBase<ServicePrincipal>, IServicePrincipalGraphHelper
    {
        private readonly IObjectTrackingService objectService;

        /// <summary>
        /// Constructor for ServicePrincipalGraphHelper
        /// </summary>
        /// <param name="settings">Settings for the graph helper</param>
        /// <param name="auditService">An instance of the Audit Service</param>
        /// <param name="objectService">An instance of the Object Tracking Service</param>
        /// <param name="graphClient">An instance of a GraphClient</param>
        /// <param name="logger">An instance of an ILogger</param>
        public ServicePrincipalGraphHelper(IGraphHelperSettings settings, IAuditService auditService, IObjectTrackingService objectService, IGraphServiceClient graphClient, ILogger<ServicePrincipalGraphHelper> logger)
                : base(settings, auditService, graphClient, logger)
        {
            this.objectService = objectService;
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
            var servicePrincipalList = new HashSet<ServicePrincipal>(ServicePrincipalComparer.Default);

            // Build first page of directory elements
            if (IsSeedRun(config))
            {
                logger.LogInformation("Seeding Service Principal objects from Graph...");
                metrics.Name = GraphOperation.FullSeed;

                collectionPage = await GetGraphSeedRequest()
                                        .GetAsync()
                                        .ConfigureAwait(false);
            }
            else
            {
                metrics.Name = GraphOperation.DeltaDiscovery;

                logger.LogInformation("Fetching Service Principal Delta objects from Graph...");

                collectionPage = new ServicePrincipalDeltaCollectionPage();
                collectionPage.InitializeNextPageRequest(GraphClient, config.DeltaLink);
                collectionPage = await collectionPage.NextPageRequest.GetAsync().ConfigureAwait(false);
            }

            servicePrincipalList.UnionWith(await PruneRemoved(context, collectionPage, metrics).ConfigureAwait(false));

            while (collectionPage.NextPageRequest != null)
            {
                collectionPage = await collectionPage.NextPageRequest.GetAsync().ConfigureAwait(false);

                servicePrincipalList.UnionWith(await PruneRemoved(context, collectionPage, metrics).ConfigureAwait(false));
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
        /// <param name="id">The Service Principal Object Id</param>
        /// <returns>Task returning a Service Principal</returns>
        public async override Task<(ServicePrincipal, IList<User>)> GetEntityWithOwners(string id)
        {
            var entity = await GraphClient.ServicePrincipals[id]
                .Request()
                .Expand("Owners")
                .GetAsync()
                .ConfigureAwait(false);

            var owners = entity
                            .Owners?
                            .Where(x => x.GetType() == typeof(User))
                            .Select(x => (User)x).ToList()
                                ?? new List<User>();

            return (entity, owners);
        }

        /// <summary>
        /// Gets Application Graph Object With Owners
        /// </summary>
        /// <param name="appId">The Application Object Id</param>
        /// <returns>Task returning an Application Object</returns>
        public async Task<Application> GetApplicationWithOwners(string appId)
        {
            var entityList = await GraphClient.Applications
                .Request()
                .Filter($"appId eq '{appId}'")
                .Expand("Owners")
                .GetAsync()
                .ConfigureAwait(false);

            return entityList.FirstOrDefault();
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

        private static bool IsSeedRun(ProcessorConfiguration config) =>
            config.RunState == RunState.Seed ||
            string.IsNullOrEmpty(config.DeltaLink);

        private async Task<IList<ServicePrincipal>> PruneRemoved(ActivityContext context, IServicePrincipalDeltaCollectionPage collectionPage, GraphOperationMetrics metrics)
        {
            IList<ServicePrincipal> pageList = collectionPage.CurrentPage ?? new List<ServicePrincipal>();
            var count = pageList.Count;

            metrics.Considered += count;

            logger.LogDebug($"\tDiscovered {count} Service Principals");

            // Build secondary list of elements that were removed from the directory
            List<ServicePrincipal> removedList = pageList.Where(x => x.AdditionalData.Keys.Contains("@removed")).ToList();

            // Report Audit Ignore messages for all the elements that were already removed from the directory
            removedList.ToList().ForEach(sp => auditService.PutIgnore(
                descriptor: new AuditDescriptor
                {
                    CorrelationId = context.CorrelationId,
                    ObjectId = sp.Id,
                    AppId = sp.AppId,
                    DisplayName = sp.DisplayName,
                },
                code: AuditCode.Deleted,
                attributeName: "AdditionalData",
                existingAttributeValue: "@removed"));

            // Update LKG (if exists) for any removed ServicePrincipals
            await PruneLastKnownGood(context, removedList).ConfigureAwait(false);

            logger.LogInformation($"\tTrimmed {removedList.Count} ServicePrincipals.");
            metrics.Removed += removedList.Count;

            // Filter list to include only those elements that are currently active
            pageList = pageList.Where(x => x.AdditionalData.Keys.Contains("@removed") == false).ToList();

            return pageList;
        }

        /// <summary>
        /// Mark service principals in LKG as deleted
        /// </summary>
        /// <param name="context">An instance of ActivityContext</param>
        /// <param name="list">The list of ServicePrincipals to mark as deleted in LKG</param>
        /// <returns>An awaitable Task</returns>
        private async Task PruneLastKnownGood(ActivityContext context, IList<ServicePrincipal> list)
        {
            foreach (var item in list)
            {
                var trackingModel = await objectService.Get<ServicePrincipalModel>(item.Id).ConfigureAwait(false);
                if (trackingModel != null)
                {
                    trackingModel.Deleted = DateTimeOffset.Now;
                    trackingModel.State = TrackingState.Untracked;
                    await objectService.Put(context, trackingModel).ConfigureAwait(false);
                }
            }
        }
    }
}
