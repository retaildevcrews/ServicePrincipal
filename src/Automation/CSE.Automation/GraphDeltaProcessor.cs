using System;
using System.Diagnostics;
using CSE.Automation.Interfaces;
using CSE.Automation.Graph;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using CSE.Automation.Services;
using System.Globalization;
using Microsoft.Graph;
using CSE.Automation.Processors;
using CSE.Automation.DataAccess;

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
            var queueConnectionString = _secretService.GetSecretValue(Constants.SPStorageConnectionString);
            var dataQueueName = _secretService.GetSecretValue(Constants.SPTrackingUpdateQueue);
            Model.ProcessorConfiguration config = null; //TODO

            var servicePrincipalResult = _graphHelper.GetDeltaGraphObjects("appId,displayName,notes",config).Result;

            string updatedDeltaLink = servicePrincipalResult.Item1;
            var servicePrincipals = servicePrincipalResult.Item2;

            IAzureQueueService azureQueue = new AzureQueueService(queueConnectionString, dataQueueName);

            int visibilityDelayGapSeconds = Int32.Parse(Environment.GetEnvironmentVariable("visibilityDelayGapSeconds"), CultureInfo.InvariantCulture);
            int queueRecordProcessThreshold = Int32.Parse(Environment.GetEnvironmentVariable("queueRecordProcessThreshold"),CultureInfo.InvariantCulture);
            
            int servicePrincipalCount = default;
            int visibilityDelay = default;

            log.LogInformation($"Processing Service Principal objects from Graph...");
            foreach (var sp in servicePrincipals)
            {
                if (String.IsNullOrWhiteSpace(sp.AppId) || String.IsNullOrWhiteSpace(sp.DisplayName))
                    continue;

                var servicePrincipal = new Model.ServicePrincipalModel()
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

                if (servicePrincipalCount % queueRecordProcessThreshold == 0 && servicePrincipalCount!=0)
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

    }
}
