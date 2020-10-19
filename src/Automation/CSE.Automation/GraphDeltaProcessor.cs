using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
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

namespace CSE.Automation
{
    public class GraphDeltaProcessor
    {
        private readonly IServicePrincipalProcessor _processor;

        public GraphDeltaProcessor(IServiceProvider serviceProvider, IServicePrincipalProcessor processor)
        {
            _processor = processor;

            ValidateServices(serviceProvider);
        }

        //private async Task<int> LaunchSeedDeltaProcessor()
        //{
        //    int visibilityDelayGapSeconds = int.Parse(Environment.GetEnvironmentVariable("visibilityDelayGapSeconds"), CultureInfo.InvariantCulture);
        //    int queueRecordProcessThreshold = int.Parse(Environment.GetEnvironmentVariable("queueRecordProcessThreshold"), CultureInfo.InvariantCulture);
        //    _processor.VisibilityDelayGapSeconds = visibilityDelayGapSeconds;
        //    _processor.QueueRecordProcessThreshold = queueRecordProcessThreshold;

        //    return await _processor.ProcessDeltas().ConfigureAwait(false);
        //}

        [FunctionName("SeedDeltaProcessorTimer")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Will add specific error in time.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Required as part of Trigger declaration.")]
        public async Task<int> SeedDeltaProcessorTimer([TimerTrigger("0 */30 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogDebug("Executing SeedDeltaProcessorTimer Function");


            var result = await _processor.ProcessDeltas().ConfigureAwait(false);
            return result;
        }

        [FunctionName("SeedDeltaProcessor")]
        public async Task<IActionResult> SeedDeltaProcessor(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogDebug("Executing SeedDeltaProcessor HttpTrigger Function");


            // TODO: If we end up with now request params needed for the seed function then remove the param and this check.
            if (req is null)
                throw new ArgumentNullException(nameof(req));

            int objectCount = await _processor.ProcessDeltas().ConfigureAwait(false);

            return new OkObjectResult($"Service Principal Objects Processed: {objectCount}");
        }

        [FunctionName("SPTrackingQueueTrigger")]
        [StorageAccount(Constants.SPStorageConnectionString)]
        public static async Task SPTrackingQueueTrigger([QueueTrigger(Constants.SPTrackingUpdateQueueAppSetting)] CloudQueueMessage msg, ILogger log)
        {
            if (msg == null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            log.LogInformation("Incoming message from SPTracking queue");

            // Made this async to adhere with Function being declared async Task.  Remove once actual processing logic is added.
            await Task.Run(() => { log.LogInformation($"Queue trigger function processed: {msg.Id}"); }).ConfigureAwait(false);

        }

        [FunctionName("SPAADQueueTrigger")]
        [StorageAccount(Constants.SPStorageConnectionString)]
        public static void SPAADQueueTrigger([QueueTrigger(Constants.SPAADUpdateQueueAppSetting)] CloudQueueMessage msg, ILogger log)
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
