// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CSE.Automation.Extensions;
using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json.Converters;

namespace CSE.Automation.Processors
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum UpdateMode
    {
        /// <summary>
        /// AAD Update logic attempts to write to AAD
        /// </summary>
        Update,

        /// <summary>
        /// AAD Update logic only reports a requested change but does not attempt to write to AAD
        /// </summary>
        ReportOnly,
    }

    internal class ServicePrincipalProcessor : DeltaProcessorBase, IServicePrincipalProcessor
    {
        public static string ConstDefaultConfigurationResourceName = "ServicePrincipalProcessorConfiguration";

        private readonly IGraphHelper<ServicePrincipal> graphHelper;
        private readonly ServicePrincipalProcessorSettings settings;
        private readonly IQueueServiceFactory queueServiceFactory;
        private readonly IObjectTrackingService objectService;
        private readonly IAuditService auditService;
        private readonly IActivityService activityService;
        private readonly IEnumerable<IModelValidator<ServicePrincipalModel>> validators;

        public ServicePrincipalProcessor(
            ServicePrincipalProcessorSettings settings,
            IGraphHelper<ServicePrincipal> graphHelper,
            IQueueServiceFactory queueServiceFactory,
            IConfigService<ProcessorConfiguration> configService,
            IObjectTrackingService objectService,
            IAuditService auditService,
            IActivityService activityService,
            IModelValidatorFactory modelValidatorFactory,
            ILogger<ServicePrincipalProcessor> logger)
            : base(configService, logger)
        {
            this.settings = settings;
            this.graphHelper = graphHelper;
            this.objectService = objectService;
            this.auditService = auditService;
            this.activityService = activityService;
            this.queueServiceFactory = queueServiceFactory;

            validators = modelValidatorFactory.Get<ServicePrincipalModel>();
        }

        public override int VisibilityDelayGapSeconds => settings.VisibilityDelayGapSeconds;
        public override int QueueRecordProcessThreshold => settings.QueueRecordProcessThreshold;
        public override Guid ConfigurationId => settings.ConfigurationId;
        public override ProcessorType ProcessorType => ProcessorType.ServicePrincipal;
        protected override string DefaultConfigurationResourceName => ConstDefaultConfigurationResourceName;

        /// REQUESTDISCOVERY
        /// <summary>
        /// Submit a request to perform a Discovery
        /// </summary>
        /// <param name="context">An instance of an <see cref="ActivityContext"/>.</param>
        /// <param name="discoveryMode">Type of discovery to perform.</param>
        /// <param name="source">The Command source.</param>
        /// <returns>A Task that may be awaited.</returns>
        public override async Task RequestDiscovery(ActivityContext context, DiscoveryMode discoveryMode, string source)
        {
            var message = new QueueMessage<RequestDiscoveryCommand>()
            {
                QueueMessageType = QueueMessageType.Data,
                Document = new RequestDiscoveryCommand
                {
                    CorrelationId = context.CorrelationId,
                    DiscoveryMode = discoveryMode,
                    Source = source,
                },
                Attempt = 0,
            };

            await queueServiceFactory
                    .Create(settings.QueueConnectionString, settings.DiscoverQueueName)
                    .Send(message, 0)
                    .ConfigureAwait(false);
        }

        /// DISCOVERYSTATUS
        /// <summary>
        /// Return the status of an activity from activityhistory.
        /// </summary>
        /// <param name="context">An instance of an <see cref="ActivityContext"/>.</param>
        /// <param name="activityId">Id of the activity to report.</param>
        /// <param name="correlationId">Correlation id of the activities to report.</param>
        /// <returns>A Task that may be awaited.</returns>
        /// <remarks>Either activityId or correlationId must be provided.</remarks>
        public override async Task<IEnumerable<ActivityHistory>> GetActivityStatus(ActivityContext context, string activityId, string correlationId)
        {
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                var activity = await activityService.Get(activityId).ConfigureAwait(false);
                return new[] { activity };
            }
            else
            {
                return await activityService.GetCorrelated(correlationId).ConfigureAwait(false);
            }
        }

        /// DISCOVER
        /// <summary>
        /// Discover changes to ServicePrincipals in the Directory.  Either perform an initial seed or a delta detection action.
        /// </summary>
        /// <param name="context">Context of the activity.</param>
        /// <param name="forceReseed">Force a reseed regardless of config runstate or deltalink.</param>
        /// <returns>A Task that returns an instance of <see cref="GraphOperationMetrics"/>.</returns>
        public override async Task<GraphOperationMetrics> DiscoverDeltas(ActivityContext context, bool forceReseed = false)
        {
            EnsureInitialized();

            if (forceReseed)
            {
                config.RunState = RunState.Seed;
            }

            // Create the queue client for when we need to post the evaluate commands
            IAzureQueueService queueService = queueServiceFactory.Create(settings.QueueConnectionString, settings.EvaluateQueueName);

            // Perform the delta query against the Graph
            // var selectFields = new[] { "appId", "displayName", "notes", "additionalData" };
            var servicePrincipalResult = await graphHelper.GetDeltaGraphObjects(context, config).ConfigureAwait(false);

            var metrics = servicePrincipalResult.metrics;
            string updatedDeltaLink = metrics.AdditionalData;
            var servicePrincipalList = servicePrincipalResult.data.ToList();

            logger.LogInformation($"Resolving Owners for ServicePrincipal objects...");
            int servicePrincipalCount = 0;

            // foreach (var sp in servicePrincipalList.Where(sp => string.IsNullOrWhiteSpace(sp.AppId) == false && string.IsNullOrWhiteSpace(sp.DisplayName) == false))
            foreach (var sp in servicePrincipalList)
            {
                var fullSP = await graphHelper.GetGraphObjectWithOwners(sp.Id).ConfigureAwait(false);
                var owners = fullSP?.Owners.Select(x => (x as User)?.UserPrincipalName).ToList();

                servicePrincipalCount++;

                if (servicePrincipalCount % 200 == 0)
                {
                    logger.LogInformation($"\tResolved {servicePrincipalCount} ServicePrincipals.");
                }

                var model = new ServicePrincipalModel()
                {
                    Id = sp.Id,
                    AppId = sp.AppId,
                    DisplayName = sp.DisplayName,
                    Notes = sp.Notes,
                    Created = DateTimeOffset.Parse(sp.AdditionalData["createdDateTime"].ToString(), CultureInfo.CurrentCulture),
                    Deleted = sp.DeletedDateTime,
                    Owners = owners,
                    ObjectType = ObjectType.ServicePrincipal,
                    ServicePrincipalType = sp.ServicePrincipalType,
                };

                var myMessage = new QueueMessage<EvaluateServicePrincipalCommand>()
                {
                    QueueMessageType = QueueMessageType.Data,
                    Document = new EvaluateServicePrincipalCommand
                    {
                        CorrelationId = context.CorrelationId,
                        Model = model,
                    },
                    Attempt = 0,
                };

                await queueService.Send(myMessage).ConfigureAwait(false);
            }

            logger.LogInformation($"{servicePrincipalCount} ServicePrincipals resolved.");

            if (config.RunState == RunState.Seed)
            {
                config.LastSeedTime = DateTimeOffset.Now;
            }
            else
            {
                config.LastDeltaRun = DateTimeOffset.Now;
            }

            config.DeltaLink = updatedDeltaLink;
            config.RunState = RunState.DeltaRun;

            await configService.Put(config).ConfigureAwait(false);

            logger.LogInformation($"Finished Processing {servicePrincipalCount} Service Principal Objects.");

            context.Activity.MergeMetrics(metrics.ToDictionary());
            await activityService.Put(context.Activity).ConfigureAwait(false);
            return metrics;
        }

        /// EVALUATE
        /// <summary>
        /// Evaluate the ServicePrincipal to determine if any changes are required.
        /// </summary>
        /// <param name="context">Context of the activity.</param>
        /// <param name="entity">Entity of type <see cref="ServicePrincipalModel"/> to evaluate.</param>
        /// <returns>Task to be awaited.</returns>
        public async Task Evaluate(ActivityContext context, ServicePrincipalModel entity)
        {
            IAzureQueueService queueService = queueServiceFactory.Create(settings.QueueConnectionString, settings.UpdateQueueName);

            var errors = validators.SelectMany(v => v.Validate(entity).Errors).ToList();
            if (errors.Count > 0)
            {
                // emit into Operations log
                var errorMsg = string.Join('\n', errors);
                logger.LogError($"ServicePrincipal {entity.Id} failed validation.\n{errorMsg}");

                // emit into Audit log, all failures
                errors.ForEach(async error => await auditService.PutFail(
                                   context: context,
                                   code: AuditCode.Fail_AttributeValidation,
                                   objectId: entity.Id,
                                   attributeName: error.PropertyName,
                                   existingAttributeValue: error.AttemptedValue != null && error.AttemptedValue.GetType() == typeof(List<string>) ? string.Join(",", error.AttemptedValue as List<string>) : error.AttemptedValue?.ToString(),
                                   message: error.ErrorMessage).ConfigureAwait(false));

                // Invalid ServicePrincipal, valid Owners, update the service principal from Owners
                if (entity.HasOwners())
                {
                    await UpdateNotesFromOwners(context, entity, queueService).ConfigureAwait(false);
                }

                // Revert the ServicePrincipal if we can
                else
                {
                    await RevertToLastKnownGood(context, entity, queueService).ConfigureAwait(false);
                }
            }

            // No errors, ServicePrincipal passes audit
            else
            {
                // remember this was the last time we saw the ServicePrincipal as 'good'
                await UpdateLastKnownGood(context, entity).ConfigureAwait(true);
                await auditService.PutPass(context, AuditCode.Pass_ServicePrincipal, entity.Id, null, null).ConfigureAwait(false);
            }
        }

        /// UPDATE
        /// <summary>
        /// Update AAD with any of the changes determined in the EVALUATE step
        /// </summary>
        /// <param name="context">Context of the activity.</param>
        /// <param name="command">The command from the activity queue.</param>
        /// <returns>A Task that returns nothing.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "All failure condition logging")]
        public async Task UpdateServicePrincipal(ActivityContext context, ServicePrincipalUpdateCommand command)
        {
            if (settings.AADUpdateMode == UpdateMode.Update)
            {
                try
                {
                    await graphHelper.PatchGraphObject(new ServicePrincipal
                    {
                        Id = command.Id,
                        Notes = command.Notes.Changed,
                    }).ConfigureAwait(true);
                }
                catch (Microsoft.Graph.ServiceException exSvc)
                {
                    logger.LogError(exSvc, $"Failed to update AAD Service Principal {command.Id}");
                    try
                    {
                        await auditService.PutFail(context, AuditCode.Fail_AADUpdate, command.Id, "Notes", command.Notes.Current, exSvc.Message).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Failed to Audit update to AAD Service Principal {command.Id}");

                        // do not rethrow, it will hide the real failure
                    }
                }

                try
                {
                    await auditService.PutChange(context, AuditCode.Change_ServicePrincipalUpdated, command.Id, "Notes", command.Notes.Current, command.Notes.Changed, command.Message).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to Audit update to AAD Service Principal {command.Id}");
                    throw;
                }
            }
            else
            {
                logger.LogInformation($"Update mode is {settings.AADUpdateMode}, ServicePrincipal {command.Id} will not be updated.");
            }
        }

        /// <summary>
        /// Update the ServicePrincipal Notes from the Owners List
        /// </summary>
        /// <param name="context">Context of the activity.</param>
        /// <param name="entity">Entity of type <see cref="ServicePrincipalModel"/>.</param>
        /// <param name="queueService">An instance of a Queue Service to send an update message.</param>
        /// <returns>A Task that returns nothing.</returns>
        private async Task UpdateNotesFromOwners(ActivityContext context, ServicePrincipalModel entity, IAzureQueueService queueService)
        {
            TrackingModel lastKnownGoodWrapper = await objectService.Get<ServicePrincipalModel>(entity.Id).ConfigureAwait(true);

            // get new value for Notes (from the list of Owners)
            // var owners = sp.Owners.Select(x => (x as User)?.UserPrincipalName);
            var ownersList = string.Join(';', entity.Owners);

            // command the AAD Update
            var updateCommand = new ServicePrincipalUpdateCommand()
            {
                Id = entity.Id,
                Notes = (entity.Notes, ownersList),
                Message = "Update Notes from Owners",
            };
            await CommandAADUpdate(context, updateCommand, queueService).ConfigureAwait(true);

            // update local entity so we have something to send to object service
            entity.Notes = ownersList;

            await UpdateLastKnownGood(context, lastKnownGoodWrapper, entity).ConfigureAwait(false);
        }

        /// <summary>
        /// Update the ServicePrincipal from the last known good state.
        /// </summary>
        /// <param name="context">Context of the activity.</param>
        /// <param name="entity">Entity of type <see cref="ServicePrincipalModel"/>.</param>
        /// <param name="queueService">An instance of a Queue Service to send an update message.</param>
        /// <returns>A Task that returns nothing.</returns>
        private async Task RevertToLastKnownGood(ActivityContext context, ServicePrincipalModel entity, IAzureQueueService queueService)
        {
            TrackingModel lastKnownGoodWrapper = await objectService.Get<ServicePrincipalModel>(entity.Id).ConfigureAwait(false);
            var lastKnownGood = TrackingModel.Unwrap<ServicePrincipalModel>(lastKnownGoodWrapper);

            // bad SP Notes, bad SP Owners, last known good found
            if (lastKnownGood != null)
            {
                logger.LogInformation($"Reverting {entity.Id} to last known good state from {lastKnownGood.LastUpdated}");

                // build the command here so we don't need to pass the delta values down the call tree
                var updateCommand = new ServicePrincipalUpdateCommand()
                {
                    Id = entity.Id,
                    Notes = (entity.Notes, lastKnownGood.Notes),
                    Message = "Revert to Last Known Good",
                };

                await CommandAADUpdate(context, updateCommand, queueService).ConfigureAwait(true);
                await UpdateLastKnownGood(context, lastKnownGoodWrapper, lastKnownGood).ConfigureAwait(false);
            }

            // oops, bad SP Notes, bad SP Owners and no last known good.
            else
            {
                await AlertInvalidPrincipal(context, entity).ConfigureAwait(false);
            }
        }

        private async Task UpdateLastKnownGood(ActivityContext context, TrackingModel trackingModel, ServicePrincipalModel model)
        {
            // if we dont have a tracking model that means we have never recorded a last known good.  create one by
            //  writing just the entity to the service.
            if (trackingModel is null)
            {
                await UpdateLastKnownGood(context, model).ConfigureAwait(true);
            }
            else
            {
                trackingModel.Entity = model;

                // make sure to write the wrapper back to get the same document updated
                await objectService.Put(context, trackingModel).ConfigureAwait(false);
            }
        }

        private async Task UpdateLastKnownGood(ActivityContext context, ServicePrincipalModel entity)
        {
            await objectService.Put(context, entity).ConfigureAwait(false);
        }

        private static async Task CommandAADUpdate(ActivityContext context, ServicePrincipalUpdateCommand command, IAzureQueueService queueService)
        {
            command.CorrelationId = context.CorrelationId;
            var message = new QueueMessage<ServicePrincipalUpdateCommand>()
            {
                QueueMessageType = QueueMessageType.Data,
                Document = command,
                Attempt = 0,
            };

            await queueService.Send(message, 0).ConfigureAwait(false);
        }

        /// <summary>
        /// Report that the system does not contain sufficient information to remediate the ServicePrincipal.
        /// </summary>
        /// <param name="context">Context of the activity.</param>
        /// <param name="entity">Entity of type <see cref="ServicePrincipalModel"/> to report.</param>
        /// <returns>Task to be awaited.</returns>
        private async Task AlertInvalidPrincipal(ActivityContext context, ServicePrincipalModel entity)
        {
            try
            {
                await auditService.PutFail(context, AuditCode.Fail_MissingOwners, entity.Id, "Notes", null).ConfigureAwait(true);
                logger.LogWarning($"AUDIT FAIL: {entity.Id} {AuditCode.Fail_MissingOwners.Description()}");
            }
            catch (Exception)
            {
                throw;
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
