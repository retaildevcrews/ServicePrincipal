using System;
using System.Globalization;
using System.Threading.Tasks;
using CSE.Automation.Interfaces;
using CSE.Automation.Processors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace CSE.Automation
{
    public class GraphDeltaProcessor
    {

        private readonly ProcessorResolver _processorResolver;
    
        public GraphDeltaProcessor(ProcessorResolver processorResolver)
        {
            _processorResolver = processorResolver;
        }

        private async Task<int> launchSeedDeltaProcessor()
        {
            int visibilityDelayGapSeconds = int.Parse(Environment.GetEnvironmentVariable("visibilityDelayGapSeconds"), CultureInfo.InvariantCulture);
            int queueRecordProcessThreshold = int.Parse(Environment.GetEnvironmentVariable("queueRecordProcessThreshold"), CultureInfo.InvariantCulture);

            ServicePrincipalProcessor spProcessor = (ServicePrincipalProcessor)_processorResolver.GetService<IDeltaProcessor>(ProcessorType.ServicePrincipal.ToString());
            spProcessor.visibilityDelayGapSeconds = visibilityDelayGapSeconds;
            spProcessor.queueRecordProcessThreshold = queueRecordProcessThreshold;

            return await spProcessor.ProcessDeltas().ConfigureAwait(false);
        }

        [FunctionName("SeedDeltaProcessorTimer")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Will add specific error in time.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Required as part of Trigger declaration.")]
        public async Task<int> SeedDeltaProcessorTimer([TimerTrigger("0 */30 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogDebug("Executing SeedDeltaProcessorTimer Function");
            return await launchSeedDeltaProcessor().ConfigureAwait(false);
        }

        [FunctionName("SeedDeltaProcessor")]
        public async Task<IActionResult> SeedServicePrincipal(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogDebug("Executing SeedDeltaProcessor HttpTrigger Function");
            // TODO: If we end up with now request params needed for the seed function then remove the param and this check.
            if (req is null)
                throw new ArgumentNullException(nameof(req));

            int objectCount = await launchSeedDeltaProcessor().ConfigureAwait(false);

            return new OkObjectResult($"Service Principal Objects Processed: {objectCount}");
        }

        [FunctionName("SPTrackingQueueTriggerFunction")]
        [StorageAccount(Constants.SPStorageConnectionString)]
        public static async Task RunSPTrackingQueueDaemon([QueueTrigger(Constants.SPTrackingUpdateQueueAppSetting)] CloudQueueMessage msg,
            [Queue(Constants.SPAADUpdateQueueAppSetting)] CloudQueue queue, ILogger log)
        {
            if (queue is null)
                throw new ArgumentNullException(nameof (queue));
            log.LogInformation("Incoming message from SPTracking queue\n");
            log.LogInformation($"C# SP Tracking Queue trigger function processed: {msg} \n");

            var newMsg = $"Following message processed from SPTracking queue:\n{msg}\n";
            await queue.AddMessageAsync(new CloudQueueMessage(newMsg)).ConfigureAwait(false);
        }

        [FunctionName("SPAADQueueTriggerFunction")]
        [StorageAccount(Constants.SPStorageConnectionString)]
        public static void RunSPAADQueueDaemon([QueueTrigger(Constants.SPAADUpdateQueueAppSetting)] CloudQueueMessage msg, ILogger log)
        {
            log.LogInformation("Incoming message from AAD queue\n");
            log.LogInformation($"C# AAD Queue trigger function processed: {msg} \n");
        }
    }
}