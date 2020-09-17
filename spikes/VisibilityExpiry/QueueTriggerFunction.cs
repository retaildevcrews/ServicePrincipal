using System;
using System.Threading;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace VisibilityExpiry
{
    public static class QueueTriggerFunction
    {
        [FunctionName("QueueTriggerFunction")]
        [StorageAccount("AzureWebJobsStorage")]
        public static void Run([QueueTrigger("outqueue")] CloudQueueMessage msg, ILogger log)
        {
            log.LogInformation("Incoming message\n");
            Thread.Sleep(5000);
            log.LogInformation($"C# Queue trigger function processed: {msg.AsString} \n");
        }
    }
}
