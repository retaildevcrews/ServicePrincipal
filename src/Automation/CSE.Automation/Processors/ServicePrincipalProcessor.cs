using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Services;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using CSE.Automation.DataAccess;
using System.Threading.Tasks;
using CSE.Automation.Properties;

namespace CSE.Automation.Processors
{
    public interface IServicePrincipalProcessor : IDeltaProcessor { }

    class ServicePrincipalProcessorSettings : DeltaProcessorSettings
    {
        public ServicePrincipalProcessorSettings(ISecretClient secretClient) : base(secretClient) { }

        [Secret(Constants.SPStorageConnectionString)]
        public string QueueConnectionString => base.GetSecret();
        [Secret(Constants.SPTrackingUpdateQueue)]
        public string QueueName => base.GetSecret();

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
        private readonly IQueueServiceFactory _queueServiceFactory;
        public ServicePrincipalProcessor(ServicePrincipalProcessorSettings settings, IGraphHelper<ServicePrincipal> graphHelper, IQueueServiceFactory queueServiceFactory, IConfigRepository repository) : base(repository)
        {
            _settings = settings;
            _graphHelper = graphHelper;
            _queueServiceFactory = queueServiceFactory;
        }

        public override int VisibilityDelayGapSeconds => _settings.VisibilityDelayGapSeconds;
        public override int QueueRecordProcessThreshold => _settings.QueueRecordProcessThreshold;
        public override Guid ConfigurationId => _settings.ConfigurationId;
        protected override byte[] DefaultConfigurationResource => Resources.ServicePrincipalProcessorConfiguration;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Console.WriteLine will be changed to logs")]
        public override async Task<int> ProcessDeltas()
        {
            EnsureInitialized();

            var servicePrincipalResult = await _graphHelper.GetDeltaGraphObjects(string.Join(',', _config.SelectFields), _config).ConfigureAwait(false);

            string updatedDeltaLink = servicePrincipalResult.Item1; //TODO save this back in Config
            var servicePrincipalList = servicePrincipalResult.Item2;

            IAzureQueueService queueService = _queueServiceFactory.Create(_settings.QueueConnectionString, _settings.QueueName);

            int servicePrincipalCount = default;
            int visibilityDelay = default;

            Console.WriteLine($"Processing Service Principal objects..."); //TODO change this to log

            foreach (var sp in servicePrincipalList)
            {
                if (String.IsNullOrWhiteSpace(sp.AppId) || String.IsNullOrWhiteSpace(sp.DisplayName))
                    continue;
                //TODO validation of service principal objects using FluentValidation
                var servicePrincipal = new ServicePrincipalModel()
                {
                    AppId = sp.AppId,
                    DisplayName = sp.DisplayName,
                    Notes = sp.Notes
                };

                var myMessage = new QueueMessage()
                {
                    QueueMessageType = QueueMessageType.Data,
                    Document = servicePrincipal,
                    Attempt = 0
                };

                if (servicePrincipalCount % QueueRecordProcessThreshold == 0 && servicePrincipalCount != 0)
                {
                    Console.WriteLine($"Processed {servicePrincipalCount} Service Principal Objects."); //TODO change this to log
                    visibilityDelay += VisibilityDelayGapSeconds;
                }
                await queueService.Send(myMessage, visibilityDelay).ConfigureAwait(false);
                servicePrincipalCount++;
            }

            _config.DeltaLink = updatedDeltaLink;
            _config.LastSeedTime = DateTime.Now;
            _config.RunState = RunState.DeltaRun;

            await _configRepository.ReplaceDocumentAsync(_config.Id, _config).ConfigureAwait(false);

            Console.WriteLine($"Finished Processing {servicePrincipalCount} Service Principal Objects."); //TODO change this to log
            return servicePrincipalCount;
        }

    }
}
