using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Services;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using CSE.Automation.DataAccess;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CSE.Automation.Properties;
using System.Text.RegularExpressions;
using FluentValidation.Results;

namespace CSE.Automation.Processors
{
    public interface IServicePrincipalProcessor : IDeltaProcessor
    {
        Task Evaluate(ActivityContext context, ServicePrincipalModel entity);
    }

    class ServicePrincipalProcessorSettings : DeltaProcessorSettings
    {
        private string _queueConnectionString;
        private string _queueName;

        public ServicePrincipalProcessorSettings(ISecretClient secretClient) : base(secretClient) { }

        [Secret(Constants.SPStorageConnectionString)]
        public string QueueConnectionString
        {
            get { return _queueConnectionString ?? base.GetSecret(); }
            set { _queueConnectionString = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "Not a localizable setting")]
        [Secret(Constants.EvaluateQueueAppSetting)]
        public string QueueName
        {
            get { return _queueName ?? base.GetSecret().ToLower(); }
            set { _queueName = value?.ToLower(); }
        }

        public override void Validate()
        {
            base.Validate();
            if (string.IsNullOrEmpty(this.QueueConnectionString)) throw new ConfigurationErrorsException($"{this.GetType().Name}: QueueConnectionString is invalid");
            if (string.IsNullOrEmpty(this.QueueName)) throw new ConfigurationErrorsException($"{this.GetType().Name}: QueueName is invalid");

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
            IAzureQueueService queueService = _queueServiceFactory.Create(_settings.QueueConnectionString, _settings.QueueName);

            var selectFields = new[] { "appId", "displayName", "notes", "owners", "notificationEmailAddresses" };
            var servicePrincipalResult = await _graphHelper.GetDeltaGraphObjects(_config, context, string.Join(',', selectFields)).ConfigureAwait(false);

            string updatedDeltaLink = servicePrincipalResult.Item1; //TODO save this back in Config
            var servicePrincipalList = servicePrincipalResult.Item2;


            int servicePrincipalCount = 0;
            int visibilityDelay = 0;

            _logger.LogInformation($"Processing Service Principal objects...");

            foreach (var sp in servicePrincipalList)
            {
                if (String.IsNullOrWhiteSpace(sp.AppId) || String.IsNullOrWhiteSpace(sp.DisplayName))
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

            var errors = _validators.SelectMany(v => v.Validate(entity).Errors).ToList();
            if (errors.Count > 0)
            {
                _logger.LogError(string.Join('\n', errors));
                await RevertToLastKnownGood(entity, context, errors).ConfigureAwait(false);
            }
            else
            {
                await UpdateLastKnownGood(entity).ConfigureAwait(false);
            }

        }


        async Task RevertToLastKnownGood(ServicePrincipalModel entity, ActivityContext context, List<ValidationFailure> errors)
        {
            var lastKnownGoodWrapper = await _objectService.Get<ServicePrincipalModel>(entity.Id).ConfigureAwait(false);
            var lastKnownGood = TrackingModel.Unwrap<ServicePrincipalModel>(lastKnownGoodWrapper);
            if (lastKnownGood != null)
            {
                errors.ForEach(async error => await _auditService.PutFailThenChange(
                   context: context,
                   objectId: entity.Id,
                   attributeName: error.PropertyName,
                   existingAttributeValue: error.AttemptedValue.ToString(),
                   updatedAttributeValue: lastKnownGood.Notes,
                   reason: error.ErrorMessage).ConfigureAwait(false));
                entity.Notes = lastKnownGood.Notes;
                await CommandAADUpdate(entity).ConfigureAwait(false);
            }

            // oops, bad SP Notes and no last known good.  
            else
            {
                var sp = await _graphHelper.GetGraphObject(entity.Id).ConfigureAwait(false);

                // no notes, no owners in SP, this is an governance error
                if (sp.Owners == null || sp.Owners.Count == 0)
                {
                    await AlertInvalidPrincipal(entity).ConfigureAwait(false);
                }
                // update entity, update last known good
                else
                {
                    var owners = sp.Owners.Select(x => (x as User)?.UserPrincipalName);
                    var ownersList = string.Join(';', owners);

                    await _auditService.PutFailThenChange(
                        context: context,
                        objectId: entity.Id,
                        attributeName: "Notes",
                        existingAttributeValue: entity.Notes,
                        updatedAttributeValue: ownersList,
                        reason: "Only Valid Email Addresses Can Be Present, Repopulating From Owners"
                        ).ConfigureAwait(false);

                    entity.Notes = ownersList;

                    if (lastKnownGood is null)
                    {
                        lastKnownGood = entity;
                        await _objectService.Put(lastKnownGood).ConfigureAwait(false);
                    }
                    else
                    {
                        lastKnownGood.Notes = ownersList;
                        await _objectService.Put(lastKnownGoodWrapper).ConfigureAwait(false);
                    }

                    // make sure to write the wrapper back to get the same document updated

                }
            }
        }

        async Task CommandAADUpdate(ServicePrincipalModel entity)
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }

        async Task AlertInvalidPrincipal(ServicePrincipalModel entity)
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }

        async Task UpdateLastKnownGood(ServicePrincipalModel entity)
        {
            await _objectService.Put(entity).ConfigureAwait(false);
        }

        // REMEDIATE
        /// <summary>
        /// Update AAD with any of the changes determined in the EVALUATE step
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task UpdateServicePrincipal(ActivityContext context)
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
