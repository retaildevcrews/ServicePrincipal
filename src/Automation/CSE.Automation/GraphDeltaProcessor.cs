﻿using System;
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
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json;

namespace CSE.Automation
{
    internal class GraphDeltaProcessor
    {
        private readonly IServicePrincipalProcessor _processor;

        public GraphDeltaProcessor(IServicePrincipalProcessor processor)
        {
            _processor = processor;
        }


        [FunctionName("Discover Deltas")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Will add specific error in time.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Required as part of Trigger declaration.")]
        public async Task<int> Deltas([TimerTrigger("0 */30 * * * *")] TimerInfo myTimer, ILogger log)
        {
            var context = new ActivityContext();
            log.LogDebug("Executing SeedDeltaProcessorTimer Function");


            var result = await _processor.DiscoverDeltas(context).ConfigureAwait(false);
            return result;
        }

        [FunctionName("Full Seed")]
        public async Task<IActionResult> FullSeed([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogDebug("Executing SeedDeltaProcessor HttpTrigger Function");

            var context = new ActivityContext();

            // TODO: If we end up with now request params needed for the seed function then remove the param and this check.
            if (req is null)
                throw new ArgumentNullException(nameof(req));
            
            int objectCount = await _processor.DiscoverDeltas(context).ConfigureAwait(false);

            return new OkObjectResult($"Service Principal Objects Processed: {objectCount}");
        }

        [FunctionName("Evaluate")]
        [StorageAccount(Constants.SPStorageConnectionString)]
        public async Task Evaluate([QueueTrigger(Constants.EvaluationQueueAppSetting)] CloudQueueMessage msg, ILogger log)
        {
            var context = new ActivityContext();

            if (msg == null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            log.LogInformation("Incoming message from SPTracking queue");
            var message = JsonConvert.DeserializeObject<QueueMessage>(msg.AsString);

            await _processor.Evaluate(context, message.Document as ServicePrincipalModel).ConfigureAwait(false);

            log.LogInformation($"Queue trigger function processed: {msg.Id}");
        }

        [FunctionName("Update AAD")]
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
    }
}
