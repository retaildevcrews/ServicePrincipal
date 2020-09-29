using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using CSE.Automation.DataAccess;
using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Processors;
using CSE.Automation.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace CSE.Automation
{
    public class GraphDeltaProcessor
    {
        private readonly ICredentialService _credService;
        private readonly ISecretClient _secretService;

        private readonly GraphHelperBase<ServicePrincipal> _graphHelper;
        private readonly DALResolver _DALResolver;
        private readonly ProcessorResolver _processorResolver;

        public GraphDeltaProcessor(ISecretClient secretClient, ICredentialService credService, GraphHelperBase<ServicePrincipal> graphHelper, DALResolver dalResolver, ProcessorResolver processorResolver)
        {
            _credService = credService;
            _secretService = secretClient;
            _graphHelper = graphHelper;
            _DALResolver = dalResolver;
            _processorResolver = processorResolver;
        }

        [FunctionName("ServicePrincipalDeltas")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Will add specific error in time.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Required as part of Trigger declaration.")]
        public void Run([TimerTrigger("0 */2 * * * *")] TimerInfo myTimer, ILogger log)
        {

            if (log == null)
                throw new ArgumentNullException(nameof(log));

            try
            {
                var kvSecretValue = _secretService.GetSecretValue("testSecret");
                Debug.WriteLine(kvSecretValue);

                var spProcessor = _processorResolver.GetService<IDeltaProcessor>(ProcessorType.ServicePrincipal.ToString());
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                Debug.WriteLine(ex.Message);
            }
        }


        [FunctionName("SeedServicePrincipal")]
        public async Task<IActionResult> SeedServicePrincipal(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            // TODO: If we end up with now request params needed for the seed function then remove the param and this check.
            if (req is null)
                throw new ArgumentNullException(nameof(req));

            var queueConnectionString = _secretService.GetSecretValue(Constants.SPStorageConnectionString);
            var dataQueueName = _secretService.GetSecretValue(Constants.SPTrackingUpdateQueue);
            ProcessorConfiguration config = null; //TODO

            var servicePrincipalResult = _graphHelper.GetDeltaGraphObjects("appId,displayName,notes", config).Result;

            string updatedDeltaLink = servicePrincipalResult.Item1;
            var servicePrincipals = servicePrincipalResult.Item2;

            IAzureQueueService azureQueue = new AzureQueueService(queueConnectionString, dataQueueName);

            int visibilityDelayGapSeconds = Int32.Parse(Environment.GetEnvironmentVariable("visibilityDelayGapSeconds"), CultureInfo.InvariantCulture);
            int queueRecordProcessThreshold = Int32.Parse(Environment.GetEnvironmentVariable("queueRecordProcessThreshold"), CultureInfo.InvariantCulture);

            int servicePrincipalCount = default;
            int visibilityDelay = default;

            log.LogInformation($"Processing Service Principal objects from Graph...");
            foreach (var sp in servicePrincipals)
            {
                if (String.IsNullOrWhiteSpace(sp.AppId) || String.IsNullOrWhiteSpace(sp.DisplayName))
                    continue;

                var servicePrincipal = new ServicePrincipalModel()
                {
                    AppId = sp.AppId,
                    DisplayName = sp.DisplayName,
                    Notes = sp.Notes
                };

                var myMessage = new Model.QueueMessage()
                {
                    QueueMessageType = Model.QueueMessageType.Data,
                    Document = servicePrincipal,
                    Attempt = 0
                };

                if (servicePrincipalCount % queueRecordProcessThreshold == 0 && servicePrincipalCount != 0)
                {
                    log.LogInformation($"Processed {servicePrincipalCount} Service Principal Objects.");
                    visibilityDelay += visibilityDelayGapSeconds;
                }
                await azureQueue.Send(myMessage, visibilityDelay).ConfigureAwait(true);
                servicePrincipalCount++;
            }
            log.LogInformation($"Finished Processing {servicePrincipalCount} Service Principal Objects.");


            return new OkObjectResult($"Success");
        }

        [FunctionName("SPTrackingQueueTriggerFunction")]
        [StorageAccount(Constants.SPStorageConnectionString)]
        public static async Task RunSPTrackingQueueDaemon([QueueTrigger(Constants.SPTrackingUpdateQueueAppSetting)] CloudQueueMessage msg,
            [Queue(Constants.SPAADUpdateQueueAppSetting)] CloudQueue queue, ILogger log)
        {
            log.LogInformation("Incoming message from SPTracking queue\n");
            log.LogInformation($"C# SP Tracking Queue trigger function processed: {msg.AsString} \n");

            var newMsg = $"Following message processed from SPTracking queue:\n{msg.AsString}\n";
            await queue.AddMessageAsync(new CloudQueueMessage(newMsg)).ConfigureAwait(false);
        }

        [FunctionName("SPAADQueueTriggerFunction")]
        [StorageAccount(Constants.SPStorageConnectionString)]
        public static void RunSPAADQueueDaemon([QueueTrigger(Constants.SPAADUpdateQueueAppSetting)] CloudQueueMessage msg, ILogger log)
        {
            log.LogInformation("Incoming message from AAD queue\n");
            log.LogInformation($"C# AAD Queue trigger function processed: {msg.AsString} \n");
        }
    }
}
