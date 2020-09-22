using System;

using System.Diagnostics;
using System.Security;
using CSE.Automation.Interfaces;
using CSE.Automation.Utilities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using CSE.Automation.Services;
using Newtonsoft.Json;
using System.Globalization;

namespace CSE.Automation
{
    public class GraphDeltaProcessor
    {
        private readonly ICredentialService _credService;
        private readonly ISecretClient _secretService;

        private readonly IGraphHelper _graphHelper;
        private readonly IDALResolver _DALResolver;

        public GraphDeltaProcessor(ISecretClient secretClient, ICredentialService credService, IGraphHelper graphHelper, IDALResolver dalResolver)
        {
            _credService = credService;
            _secretService = secretClient;
            _graphHelper = graphHelper;
            _DALResolver = dalResolver;
        }

        [FunctionName("ServicePrincipalDeltas")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Will add specific error in time.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Required as part of Trigger declaration.")]
        public void Run([TimerTrigger("0 */2 * * * *")] TimerInfo myTimer, ILogger log)
        {
            try
            {
                var kvSecret = _secretService.GetSecret("testSecret");
                SecureString secureValue = _secretService.GetSecretValue("testSecret");
                Debug.WriteLine(SecureStringHelper.ConvertToUnsecureString(secureValue));
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
            var servicePrincipals = _graphHelper.SeedServicePrincipalDeltaAsync("appId,displayName,notes").Result;
            
            IAzureQueueService azureQueue = new AzureQueueService(
                SecureStringHelper.ConvertToUnsecureString(queueConnectionString),
                SecureStringHelper.ConvertToUnsecureString(dataQueueName));

            int visibilityDelayGapSeconds = Int32.Parse(Environment.GetEnvironmentVariable("visibilityDelayGapSeconds"), CultureInfo.InvariantCulture);
            int queueRecordProcessThreshold = Int32.Parse(Environment.GetEnvironmentVariable("queueRecordProcessThreshold"),CultureInfo.InvariantCulture);
            
            int servicePrincipalCount = default;
            int visibilityDelay = default;

            log.LogInformation($"Processing Service Principal objects from Graph...");
            foreach (var sp in servicePrincipals)
            {
                if (String.IsNullOrWhiteSpace(sp.AppId) || String.IsNullOrWhiteSpace(sp.DisplayName))
                    continue;

                var servicePrincipal = new Model.ServicePrincipal()
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
