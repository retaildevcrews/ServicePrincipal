using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Properties;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace CSE.Automation.Processors
{
    interface IServicePrincipalProcessor : IDeltaProcessor
    {
        Task Evaluate(ActivityContext context, ServicePrincipalModel entity);
        Task UpdateServicePrincipal(ActivityContext context, ServicePrincipalUpdateCommand command);
    }

    internal class ServicePrincipalProcessorSettings : DeltaProcessorSettings
    {
        private string _queueConnectionString;
        private string _evaluateQueueName;
        private string _updateQueueName;

        public ServicePrincipalProcessorSettings(ISecretClient secretClient) : base(secretClient) { }

        [Secret(Constants.SPStorageConnectionString)]
        public string QueueConnectionString
        {
            get => _queueConnectionString ?? base.GetSecret();
            set => _queueConnectionString = value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "Not a localizable setting")]
        public string EvaluateQueueName
        {
            get { return _evaluateQueueName; }
            set { _evaluateQueueName = value?.ToLower(); }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "Not a localizable setting")]
        public string UpdateQueueName
        {
            get { return _updateQueueName; }
            set { _updateQueueName = value?.ToLower(); }
        }

        public override void Validate()
        {
            base.Validate();
            if (string.IsNullOrEmpty(this.QueueConnectionString)) throw new ConfigurationErrorsException($"{this.GetType().Name}: QueueConnectionString is invalid");
            if (string.IsNullOrEmpty(this.EvaluateQueueName)) throw new ConfigurationErrorsException($"{this.GetType().Name}: EvaluateQueueName is invalid");
            if (string.IsNullOrEmpty(this.UpdateQueueName)) throw new ConfigurationErrorsException($"{this.GetType().Name}: UpdateQueueName is invalid");
        }
    }

    internal class ServicePrincipalProcessor : DeltaProcessorBase, IServicePrincipalProcessor
    {
        private readonly IGraphHelper<ServicePrincipal> _graphHelper;
        private readonly ServicePrincipalProcessorSettings _settings;
        private readonly ILogger _logger;
        private readonly IQueueServiceFactory _queueServiceFactory;
        private readonly IObjectTrackingService _objectService;
        private readonly IAuditService _auditService;
        private readonly IEnumerable<IModelValidator<ServicePrincipalModel>> _validators;

        public ServicePrincipalProcessor(ServicePrincipalProcessorSettings settings,
                                            IGraphHelper<ServicePrincipal> graphHelper,
                                            IQueueServiceFactory queueServiceFactory,
                                            IConfigService<ProcessorConfiguration> configService,
                                            IObjectTrackingService objectService,
                                            IAuditService auditService,
                                            IModelValidatorFactory modelValidatorFactory,
                                            ILogger<ServicePrincipalProcessor> logger) : base(configService)
        {
            _settings = settings;
            _graphHelper = graphHelper;
            _objectService = objectService;
            _auditService = auditService;
            _logger = logger;

            _queueServiceFactory = queueServiceFactory;

            _validators = modelValidatorFactory.Get<ServicePrincipalModel>();

        }

        public override int VisibilityDelayGapSeconds => _settings.VisibilityDelayGapSeconds;
        public override int QueueRecordProcessThreshold => _settings.QueueRecordProcessThreshold;
        public override Guid ConfigurationId => _settings.ConfigurationId;
        public override ProcessorType ProcessorType => ProcessorType.ServicePrincipal;
        protected override byte[] DefaultConfigurationResource => Resources.ServicePrincipalProcessorConfiguration;

        // DISCOVER
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Console.WriteLine will be changed to logs")]
        public override async Task<int> DiscoverDeltas(ActivityContext context, bool forceReseed = false)
        {
            EnsureInitialized();

            if (forceReseed)
            {
                _config.RunState = RunState.SeedAndRun;
            }
            
            IAzureQueueService queueService = _queueServiceFactory.Create(_settings.QueueConnectionString, _settings.EvaluateQueueName);

            //var selectFields = new[] { "appId", "displayName", "notes", "additionalData" };
            var servicePrincipalResult = await _graphHelper.GetDeltaGraphObjects(context, _config, /*string.Join(',', selectFields)*/ null).ConfigureAwait(false);

            var updatedDeltaLink = servicePrincipalResult.Item1; //TODO save this back in Config
            var servicePrincipalList = servicePrincipalResult.Item2;


            var servicePrincipalCount = 0;
            var visibilityDelay = 0;

            _logger.LogInformation($"Processing Service Principal objects...");

            foreach (var sp in servicePrincipalList)
            {
                if (string.IsNullOrWhiteSpace(sp.AppId) || string.IsNullOrWhiteSpace(sp.DisplayName))
                {
                    continue;
                }


                var myMessage = new QueueMessage<ServicePrincipalModel>()
                {
                    QueueMessageType = QueueMessageType.Data,
                    Document = new ServicePrincipalModel()
                    {
                        Id = sp.Id,
                        AppId = sp.AppId,
                        DisplayName = sp.DisplayName,
                        Notes = sp.Notes,
#pragma warning disable CA1305 // Specify IFormatProvider
                        Created = DateTimeOffset.Parse(sp.AdditionalData["createdDateTime"].ToString()),
#pragma warning restore CA1305 // Specify IFormatProvider
                        Deleted = sp.DeletedDateTime,
                        // we don't have owners yet
                    },
                    Attempt = 0
                };

                if (servicePrincipalCount % QueueRecordProcessThreshold == 0 && servicePrincipalCount != 0)
                {
                    _logger.LogInformation($"Processed {servicePrincipalCount} Service Principal Objects.");
                    visibilityDelay += VisibilityDelayGapSeconds;
                }
                await queueService.Send(myMessage, visibilityDelay).ConfigureAwait(false);
                servicePrincipalCount++;
            }

            _config.DeltaLink = updatedDeltaLink;
            _config.LastSeedTime = DateTimeOffset.Now;
            _config.RunState = RunState.DeltaRun;

            await _configService.Put(_config).ConfigureAwait(false);

            _logger.LogInformation($"Finished Processing {servicePrincipalCount} Service Principal Objects.");
            return servicePrincipalCount;
        }

        // EVALUATE
        /// <summary>
        /// Evalute the ServicePrincipal to determine if any changes are required.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Evaluate(ActivityContext context, ServicePrincipalModel entity)
        {
            // 1. if Notes field is blank
            //      - update Notes field from Owners
            //      - write audit
            // 2. if Notes field is not blank
            //      - if value is not email 
            //          set value to owners field
            //          write audit
            //      - if value is email
            //          validate the value is a valid AAD user
            //          if value fails AAD check
            //              set value to owners field
            //              write audit
            //await UpdateLastKnownGood(entity).ConfigureAwait(false);

            IAzureQueueService queueService = _queueServiceFactory.Create(_settings.QueueConnectionString, _settings.UpdateQueueName);

            var errors = _validators.SelectMany(v => v.Validate(entity).Errors).ToList();
            if (errors.Count > 0)
            {
                // emit into Operations log
                var errorMsg = string.Join('\n', errors);
                _logger.LogError($"ServicePrincipal {entity.Id} failed validation.\n{errorMsg}");

                // emit into Audit log
                errors.ForEach(async error => await _auditService.PutFail(
                                   context: context,
                                   objectId: entity.Id,
                                   attributeName: error.PropertyName,
                                   existingAttributeValue: error.AttemptedValue?.ToString(),
                                   reason: error.ErrorMessage).ConfigureAwait(false));

                // Revert the principal if we can
                await RevertToLastKnownGood(context, entity, queueService).ConfigureAwait(false);
            }
            else
            {
                // remember this was the last time we saw the prinicpal as 'good'
                await UpdateLastKnownGood(context, entity).ConfigureAwait(false);
            }

        }

        async Task RevertToLastKnownGood(ActivityContext context, ServicePrincipalModel entity, IAzureQueueService queueService)
        {
            TrackingModel lastKnownGoodWrapper = await _objectService.Get<ServicePrincipalModel>(entity.Id).ConfigureAwait(false);
            var lastKnownGood = TrackingModel.Unwrap<ServicePrincipalModel>(lastKnownGoodWrapper);
            if (lastKnownGood != null)
            {
                // build the command here so we don't need to pass the delta values down the call tree
                var updateCommand = new ServicePrincipalUpdateCommand()
                {
                    Id = entity.Id,
                    Notes = (entity.Notes, lastKnownGood.Notes),
                    Reason = "Revert to Last Known Good"
                };

                await CommandAADUpdate(context, updateCommand, queueService).ConfigureAwait(false);
                await UpdateLastKnownGood(context, lastKnownGoodWrapper, lastKnownGood).ConfigureAwait(false);
            }

            // oops, bad SP Notes and no last known good.  
            else
            {
                var sp = await _graphHelper.GetGraphObject(entity.Id).ConfigureAwait(false);

                // no notes, no owners in SP, this is an governance error
                if (sp.Owners == null || sp.Owners.Count == 0)
                {
                    await AlertInvalidPrincipal(context, entity).ConfigureAwait(false);
                }

                // update ServicePrincipal from ServicePrincipal.Owners, update last known good
                else
                {
                    // get new value for Notes (from the list of Owners)
                    var owners = sp.Owners.Select(x => (x as User)?.UserPrincipalName);
                    var ownersList = string.Join(';', owners);

                    // command the AAD Update
                    var updateCommand = new ServicePrincipalUpdateCommand()
                    {
                        Id = entity.Id,
                        Notes = (entity.Notes, ownersList),
                        Reason = "Update Notes from Owners"
                    };
                    await CommandAADUpdate(context, updateCommand, queueService).ConfigureAwait(false);

                    // update local entity so we have something to send to object service
                    entity.Notes = ownersList;
                    await UpdateLastKnownGood(context, lastKnownGoodWrapper, entity).ConfigureAwait(false);
                }
            }
        }

        async Task UpdateLastKnownGood(ActivityContext context, TrackingModel trackingModel, ServicePrincipalModel model)
        {
            // if we dont have a tracking model that means we have never recorded a last known good.  create one by
            //  writing just the entity to the service.
            if (trackingModel is null)
            {
                await UpdateLastKnownGood(context, model).ConfigureAwait(false);
            }
            else
            {
                trackingModel.Entity = model;
                // make sure to write the wrapper back to get the same document updated
                await _objectService.Put(context, trackingModel).ConfigureAwait(false);
            }
        }

        async Task UpdateLastKnownGood(ActivityContext context, ServicePrincipalModel entity)
        {
            await _objectService.Put(context, entity).ConfigureAwait(false);
        }

        static async Task CommandAADUpdate(ActivityContext context, ServicePrincipalUpdateCommand command, IAzureQueueService queueService)
        {
            command.CorrelationId = context.ActivityId.ToString();
            var message = new QueueMessage<ServicePrincipalUpdateCommand>()
            {
                QueueMessageType = QueueMessageType.Data,
                Document = command,
                Attempt = 0
            };

            await queueService.Send(message, 0).ConfigureAwait(false);
        }

        async Task AlertInvalidPrincipal(ActivityContext context, ServicePrincipalModel entity)
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }




        // REMEDIATE
        /// <summary>
        /// Update AAD with any of the changes determined in the EVALUATE step
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "All failure condition logging")]
        public async Task UpdateServicePrincipal(ActivityContext context, ServicePrincipalUpdateCommand command)
        {
            try
            {
                await _graphHelper.PatchGraphObject(new ServicePrincipal
                {
                    Id = command.Id,
                    Notes = command.Notes.Item2
                }).ConfigureAwait(false);
            }
            catch (Microsoft.Graph.ServiceException exSvc)
            {
                _logger.LogError(exSvc, $"Failed to update AAD Service Principal {command.Id}");
                try
                {
                    await _auditService.PutFail(context, command.Id, "Notes", command.Notes.Current, $"Failed to update Notes field: {exSvc.Message}").ConfigureAwait(false);
                }
                catch (Exception)
                {
                    _logger.LogError(exSvc, $"Failed to Audit update to AAD Service Principal {command.Id}");
                    // do not rethrow, it will hide the real failure
                }
            }

            try
            {
                await _auditService.PutChange(context, command.Id, "Notes", command.Notes.Item1, command.Notes.Item2, command.Reason).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to Audit update to AAD Service Principal {command.Id}");
                throw;
            }

        }
    }
}
