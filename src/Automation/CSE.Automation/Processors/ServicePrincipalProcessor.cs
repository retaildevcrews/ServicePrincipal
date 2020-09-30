using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Services;
using Microsoft.Graph;
using System;
using System.Threading.Tasks;

namespace CSE.Automation.Processors
{
    public class ServicePrincipalProcessor : DeltaProcessorBase
    {
        private readonly ISecretClient _secretService;
        private readonly GraphHelperBase<ServicePrincipal> _graphHelper;

        public int visibilityDelayGapSeconds { get; set; }
        public int queueRecordProcessThreshold { get; set; }

        public ServicePrincipalProcessor(IDAL configDAL, ISecretClient secretClient, GraphHelperBase<ServicePrincipal> graphHelper,
            int visibilityDelayGapSeconds, int queueRecordProcessThreshold) : base(configDAL)
        {
            _uniqueId = "02a54ac9-441e-43f1-88ee-fde420db2559";
            InitializeProcessor(ProcessorType.ServicePrincipal);

            _secretService = secretClient;
            _configDAL = configDAL;
            _graphHelper = graphHelper;
            this.visibilityDelayGapSeconds = visibilityDelayGapSeconds;
            this.queueRecordProcessThreshold = queueRecordProcessThreshold;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Console.WriteLine will be changed to logs")]
        public override async Task ProcessDeltas()
        {
            var queueConnectionString = _secretService.GetSecretValue(Constants.SPStorageConnectionString);
            var dataQueueName = _secretService.GetSecretValue(Constants.SPTrackingUpdateQueue);

           
            var servicePrincipalResult = await _graphHelper.GetDeltaGraphObjects("appId,displayName,notes", _config).ConfigureAwait(false);

            string updatedDeltaLink = servicePrincipalResult.Item1; //TODO save this back in Config
            var servicePrincipalList = servicePrincipalResult.Item2;

            IAzureQueueService azureQueue = new AzureQueueService(queueConnectionString, dataQueueName);

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

                if (servicePrincipalCount % queueRecordProcessThreshold == 0 && servicePrincipalCount != 0)
                {
                    Console.WriteLine($"Processed {servicePrincipalCount} Service Principal Objects."); //TODO change this to log
                    visibilityDelay += visibilityDelayGapSeconds;
                }
                await azureQueue.Send(myMessage, visibilityDelay).ConfigureAwait(false);
                servicePrincipalCount++;
            }

            _config.DeltaLink = updatedDeltaLink;
            _config.LastSeedTime = DateTime.Now;
            _config.RunState = RunState.DeltaRun;

            await _configDAL.ReplaceDocumentAsync<ProcessorConfiguration>(_config.Id, _config, _config.ConfigType.ToString()).ConfigureAwait(false);

            Console.WriteLine($"Finished Processing {servicePrincipalCount} Service Principal Objects."); //TODO change this to log
        }

    }
}
