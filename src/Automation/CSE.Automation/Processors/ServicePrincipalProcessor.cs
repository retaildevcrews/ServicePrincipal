﻿// Copyright (c) Microsoft Corporation. All rights reserved.
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
using CSE.Automation.Model.Commands;
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

        private readonly IServicePrincipalGraphHelper graphHelper;
        private readonly IServicePrincipalProcessorSettings settings;
        private readonly IQueueServiceFactory queueServiceFactory;
        private readonly IObjectTrackingService objectService;
        private readonly IAuditService auditService;
        private readonly IActivityService activityService;
        private readonly IEnumerable<IModelValidator<ServicePrincipalModel>> validators;

        public ServicePrincipalProcessor(
            IServicePrincipalProcessorSettings settings,
            IServicePrincipalGraphHelper graphHelper,
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
        /// <param name="activityId">ObjectId of the activity to report.</param>
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

            // foreach (var sp in servicePrincipalList.Where(sp => string.IsNullOrWhiteSpace(sp.ObjectId) == false && string.IsNullOrWhiteSpace(sp.DisplayName) == false))
            foreach (var sp in servicePrincipalList)
            {
                IList<string> ownerNames = null;

                // Get the list of owners from the ServicePrincipal
                try
                {
                    var (_, ownerList) = await graphHelper.GetEntityWithOwners(sp.Id).ConfigureAwait(false);
                    ownerNames = ownerList.Select(x => x.UserPrincipalName).ToList();
                }
                catch (Microsoft.Graph.ServiceException svcEx)
                {
                    logger.LogWarning(svcEx, $"Failed to get Owners on ServicePrincipal ({sp.Id})");
                }

                // If no owners found on ServicePrincipal AND this is an Application ServicePrincipal, try to get the owners from the Application Object
                if (ownerNames.IsEmpty() && string.Equals(sp.ServicePrincipalType, "Application", StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        var appObject = await graphHelper.GetApplicationWithOwners(sp.AppId).ConfigureAwait(false);
                        if (appObject != null)
                        {
                            ownerNames = appObject.Owners.Select(x => (x as User)?.UserPrincipalName).ToList();
                        }
                    }
                    catch (Microsoft.Graph.ServiceException svcEx)
                    {
                        logger.LogWarning(svcEx, $"Failed to get Owners on Application ({sp.AppId})");
                    }
                }

                servicePrincipalCount++;

                if (servicePrincipalCount % 200 == 0)
                {
                    logger.LogInformation($"\tResolved {servicePrincipalCount} ServicePrincipals.");
                }

                DateTimeOffset? createdDateTime = null;
                if (sp.AdditionalData.TryGetValue("createdDateTime", out var value))
                {
                    if (DateTimeOffset.TryParse(value.ToString(), out var dateValue))
                    {
                        createdDateTime = dateValue;
                    }
                }

                var model = new ServicePrincipalModel()
                {
                    Id = sp.Id,
                    AppId = sp.AppId,
                    DisplayName = sp.DisplayName,
                    Notes = sp.Notes,
                    Created = createdDateTime,
                    Deleted = sp.DeletedDateTime,
                    Owners = ownerNames,
                    ObjectType = ObjectType.ServicePrincipal,
                    ServicePrincipalType = sp.ServicePrincipalType,
                };

                var myMessage = new QueueMessage<ServicePrincipalEvaluateCommand>()
                {
                    QueueMessageType = QueueMessageType.Data,
                    Document = new ServicePrincipalEvaluateCommand
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
            TrackingModel trackingModel = await objectService.Get<ServicePrincipalModel>(entity.Id).ConfigureAwait(false);

            if (errors.Count > 0)
            {
                // emit into Operations log
                var errorMsg = string.Join('\n', errors);
                logger.LogError($"ServicePrincipal {entity.Id} failed validation.\n{errorMsg}");

                // emit into Audit log, all failures
                errors.ForEach(async error =>
                {
                    // Check for more specific audit fail code.  Default to AttributeValidation.
                    if (Enum.TryParse(error.ErrorCode, true, out AuditCode code) == false)
                    {
                        code = AuditCode.AttributeValidation;
                    }

                    await auditService.PutFail(
                        descriptor: new AuditDescriptor
                        {
                            CorrelationId = context.CorrelationId,
                            ObjectId = entity.Id,
                            AppId = entity.AppId,
                            DisplayName = entity.DisplayName,
                        },
                        code: code,
                        attributeName: error.PropertyName,
#pragma warning disable SA1118 // Parameter should not span multiple lines
                        existingAttributeValue: error.AttemptedValue != null &&
                                                error.AttemptedValue.GetType() == typeof(List<string>)
                            ? string.Join(",", error.AttemptedValue as List<string> ?? new List<string>())
                            : error.AttemptedValue?.ToString(),
#pragma warning restore SA1118 // Parameter should not span multiple lines
                        message: error.ErrorMessage).ConfigureAwait(false);
                });

                // attempt remediation
                await RemediateServicePrincipal(context, trackingModel, entity, queueService).ConfigureAwait(false);
            }

            // No errors, ServicePrincipal passes audit
            else
            {
                // remember this was the last time we saw the ServicePrincipal as 'good'
                await UpdateLastKnownGood(context, trackingModel, entity).ConfigureAwait(true);
                var descriptor = new AuditDescriptor
                {
                    CorrelationId = context.CorrelationId,
                    ObjectId = entity.Id,
                    AppId = entity.AppId,
                    DisplayName = entity.DisplayName,
                };
                await auditService.PutPass(descriptor, AuditCode.Pass, null, null).ConfigureAwait(false);
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
                var auditEntryDescriptor = new AuditDescriptor
                {
                    CorrelationId = context.CorrelationId,
                    ObjectId = command.Entity.Id,
                    AppId = command.Entity.AppId,
                    DisplayName = command.Entity.DisplayName,
                };

                try
                {
                    logger.LogInformation($"{command.Entity.Id} ({command.Entity.DisplayName}) {command.Action.Description()}");

                    await graphHelper.PatchGraphObject(new ServicePrincipal
                    {
                        Id = command.Entity.Id,
                        Notes = command.Notes.Changed,
                    }).ConfigureAwait(true);

                    // once AAD is updated, we can update LKG
                    command.Entity.Notes = command.Notes.Changed;

                    TrackingModel trackingModel = await objectService.Get<ServicePrincipalModel>(command.Entity.Id).ConfigureAwait(false);

                    await UpdateLastKnownGood(context, trackingModel, command.Entity).ConfigureAwait(false);
                }
                catch (Microsoft.Graph.ServiceException exSvc)
                {
                    logger.LogError(exSvc, $"Failed to update AAD Service Principal {command.Entity.Id} ({command.Entity.ServicePrincipalType})");
                    try
                    {
                        await auditService.PutFail(auditEntryDescriptor, AuditCode.AADUpdate, "Notes", command.Notes.Current, exSvc.Message).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Failed to Audit update to AAD Service Principal {command.Entity.Id} ({command.Entity.ServicePrincipalType})");

                        // do not rethrow, it will hide the real failure
                        return;
                    }
                }

                try
                {
                    await auditService.PutChange(auditEntryDescriptor, AuditCode.Updated, "Notes", command.Notes.Current, command.Notes.Changed, command.Action.Description()).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to Audit update to AAD Service Principal {command.Entity.Id} ({command.Entity.ServicePrincipalType})");
                    throw;
                }
            }
            else
            {
                logger.LogInformation($"Update mode is {settings.AADUpdateMode}, {command.Entity.Id} ({command.Entity.ServicePrincipalType}) will not be updated.");
            }
        }

        private async Task RemediateServicePrincipal(ActivityContext context, TrackingModel trackingModel, ServicePrincipalModel entity, IAzureQueueService queueService)
        {
            if (trackingModel != null)
            {
                await RemediateFromLastKnownGood(context, entity, trackingModel, queueService).ConfigureAwait(false);
            }
            else if (entity.HasOwners())
            {
                await RemediateFromOwners(context, entity, queueService).ConfigureAwait(false);
            }
            else
            {
                await AlertInvalidPrincipal(context, entity).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Update the ServicePrincipal from the last known good state.
        /// </summary>
        /// <param name="context">Context of the activity.</param>
        /// <param name="entity">Original ServicePrincipalModel</param>
        /// <param name="lastKnownGood">ServicePrincipalModel from Last Known Good</param>
        /// <param name="queueService">An instance of a Queue Service to send an update message.</param>
        /// <returns>A Task that returns nothing.</returns>
        private static async Task RemediateFromLastKnownGood(ActivityContext context, ServicePrincipalModel entity, TrackingModel lastKnownGood, IAzureQueueService queueService)
        {
            var lastKnownGoodEntity = TrackingModel.Unwrap<ServicePrincipalModel>(lastKnownGood);

            // build the command here so we don't need to pass the delta values down the call tree
            var updateCommand = new ServicePrincipalUpdateCommand()
            {
                Entity = lastKnownGoodEntity,
                LastKnownGoodTime = lastKnownGood.LastUpdated,
                Notes = (entity.Notes, lastKnownGoodEntity.Notes),
                Action = ServicePrincipalUpdateAction.Revert, // "Revert to Last Known Good",
            };

            await CommandAADUpdate(context, updateCommand, queueService).ConfigureAwait(true);
        }

        /// <summary>
        /// Update the ServicePrincipal Notes from the Owners List
        /// </summary>
        /// <param name="context">Context of the activity.</param>
        /// <param name="entity">Entity of type <see cref="ServicePrincipalModel"/>.</param>
        /// <param name="queueService">An instance of a Queue Service to send an update message.</param>
        /// <returns>A Task that returns nothing.</returns>
        private static async Task RemediateFromOwners(ActivityContext context, ServicePrincipalModel entity, IAzureQueueService queueService)
        {
            // get new value for Notes (from the list of Owners)
            // var owners = sp.Owners.Select(x => (x as User)?.UserPrincipalName);
            var ownersList = string.Join(';', entity.Owners);

            // command the AAD Update
            var updateCommand = new ServicePrincipalUpdateCommand()
            {
                Entity = entity,
                Notes = (entity.Notes, ownersList),
                Action = ServicePrincipalUpdateAction.Update, // "Update Notes from Owners",
            };
            await CommandAADUpdate(context, updateCommand, queueService).ConfigureAwait(true);
        }

        /// <summary>
        /// Update the LKG document
        /// </summary>
        /// <param name="context">Context of the activity.</param>
        /// <param name="trackingModel">LKG wrapper of the entity</param>
        /// <param name="entity">Entity of type <see cref="ServicePrincipalModel"/>.</param>
        /// <returns>A Task that returns nothing.</returns>
        private async Task UpdateLastKnownGood(ActivityContext context, TrackingModel trackingModel, ServicePrincipalModel entity)
        {
            // if we don't have a tracking entity that means we have never recorded a last known good.  create one by
            //  writing just the entity to the service.
            if (trackingModel is null)
            {
                await objectService.Put(context, entity).ConfigureAwait(false);
            }
            else
            {
                trackingModel.Entity = entity;

                // make sure to write the wrapper back to get the same document updated
                await objectService.Put(context, trackingModel).ConfigureAwait(false);
            }
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Catch all for missing audit messages")]
        private async Task AlertInvalidPrincipal(ActivityContext context, ServicePrincipalModel entity)
        {
            var failMessage = $"AUDIT FAIL: {entity.Id} {AuditCode.MissingOwners.Description()}";

            try
            {
                var auditEntryDescriptor = new AuditDescriptor
                {
                    CorrelationId = context.CorrelationId,
                    ObjectId = entity.Id,
                    AppId = entity.AppId,
                    DisplayName = entity.DisplayName,
                };
                await auditService.PutFail(auditEntryDescriptor, AuditCode.MissingOwners, "Notes", null).ConfigureAwait(true);
                logger.LogWarning(failMessage);
            }
            catch (Exception)
            {
                logger.LogError($"Failed to log audit message: {failMessage}");
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
