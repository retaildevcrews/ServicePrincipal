using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Processors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json;

namespace CSE.Automation
{
    internal class GraphDeltaProcessor
    {
        private readonly IServicePrincipalProcessor _processor;
        private readonly ILogger _logger;
        public GraphDeltaProcessor(IServiceProvider serviceProvider, IServicePrincipalProcessor processor, ILogger<GraphDeltaProcessor> logger)
        {
            _processor = processor;
            _logger = logger;
            ValidateServices(serviceProvider);
        }


        [FunctionName("DiscoverDeltas")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Will add specific error in time.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Required as part of Trigger declaration.")]
        public async Task Deltas([TimerTrigger(Constants.DeltaDiscoverySchedule)] TimerInfo myTimer, ILogger log)
        {
            var context = new ActivityContext();
            log.LogDebug("Executing SeedDeltaProcessorTimer Function");


            var result = await _processor.DiscoverDeltas(context, false).ConfigureAwait(false);
            log.LogInformation($"Deltas: {result} ServicePrincipals discovered.");
        }

        [FunctionName("FullSeed")]
        public async Task<IActionResult> FullSeed([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogDebug("Executing SeedDeltaProcessor HttpTrigger Function");

            var context = new ActivityContext();

            // TODO: If we end up with now request params needed for the seed function then remove the param and this check.
            if (req is null)
                throw new ArgumentNullException(nameof(req));
            
            int objectCount = await _processor.DiscoverDeltas(context, true).ConfigureAwait(false);

            return new OkObjectResult($"Service Principal Objects Processed: {objectCount}");
        }

        [FunctionName("Evaluate")]
        [StorageAccount(Constants.SPStorageConnectionString)]
        public async Task Evaluate([QueueTrigger(Constants.EvaluateQueueAppSetting)] CloudQueueMessage msg, ILogger log)
        {
            var context = new ActivityContext();

            if (msg == null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            log.LogInformation("Incoming message from SPTracking queue");
            var message = JsonConvert.DeserializeObject<QueueMessage<ServicePrincipalModel>>(msg.AsString);

            await _processor.Evaluate(context, message.Document as ServicePrincipalModel).ConfigureAwait(false);

            log.LogInformation($"Queue trigger function processed: {msg.Id}");
        }

        [FunctionName("UpdateAAD")]
        [StorageAccount(Constants.SPStorageConnectionString)]
        public static void UpdateAAD([QueueTrigger(Constants.SPAADUpdateQueueAppSetting)] CloudQueueMessage msg, ILogger log)
        {
            if (msg == null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            log.LogInformation("Incoming message from AAD queue");
            log.LogInformation($"C# AAD Queue trigger function processed: {msg.AsString}");
        }

        #region RUNTIME VALIDATION
        private static void ValidateServices(IServiceProvider serviceProvider)
        {
            var repositories = serviceProvider.GetServices<IRepository>();
            var hasFailingTest = false;

            foreach (var repository in repositories)
            {
                var testPassed = repository.Test().Result;
                hasFailingTest = testPassed == false || hasFailingTest;

                var result = testPassed
                    ? "Passed"
                    : "Failed";
                var message = $"Repository test for {repository.Id} {result}";
                if (testPassed)
                {
                    Trace.TraceInformation(message);
                }
                else
                {
                    Trace.TraceError(message);
                }
            }

            if (hasFailingTest)
                throw new ApplicationException($"One or more repositories failed test.");
        }
        #endregion
    }
}
