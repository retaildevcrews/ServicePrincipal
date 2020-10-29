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
        private readonly IServiceProvider _serviceProvider;
        public GraphDeltaProcessor(IServiceProvider serviceProvider, IServicePrincipalProcessor processor, ILogger<GraphDeltaProcessor> logger)
        {
            _processor = processor;
            _logger = logger;
            _serviceProvider = serviceProvider;
            ValidateServices(serviceProvider);
        }


        [FunctionName("DiscoverDeltas")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Will add specific error in time.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Required as part of Trigger declaration.")]
        public async Task Deltas([TimerTrigger(Constants.DeltaDiscoverySchedule)] TimerInfo myTimer, ILogger log)
        {
            var context = new ActivityContext("Delta Detection");
            log.LogDebug("Executing SeedDeltaProcessorTimer Function");


            var metrics = await _processor.DiscoverDeltas(context, false).ConfigureAwait(false);
            context.End();
            log.LogInformation($"Deltas: {metrics.Found} ServicePrincipals discovered in {context.ElapsedTime}.");
        }

        [FunctionName("FullSeed")]
        public async Task<IActionResult> FullSeed([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogDebug("Executing SeedDeltaProcessor HttpTrigger Function");

            try
            {
                using var context = new ActivityContext("Full Seed").WithLock(_processor);

                // TODO: If we end up with now request params needed for the seed function then remove the param and this check.
                if (req is null)
                {
                    throw new ArgumentNullException(nameof(req));
                }

                var metrics = await _processor.DiscoverDeltas(context, true).ConfigureAwait(false);
                context.End();

                var result = new
                {
                    Operation = "Full Seed",
                    metrics.Considered,
                    Ignored = metrics.Removed,
                    metrics.Found,
                    context.ElapsedTime,
                };

                return new JsonResult(result);
            }
            catch (Exception)
            {
                // LOG CANNOT GET LOCK
            }
        }

        [FunctionName("Evaluate")]
        [StorageAccount(Constants.SPStorageConnectionString)]
        public async Task Evaluate([QueueTrigger(Constants.EvaluateQueueAppSetting)] CloudQueueMessage msg, ILogger log)
        {
            var context = new ActivityContext("Evaluate Service Principal");
            try
            {

                if (msg == null)
                {
                    throw new ArgumentNullException(nameof(msg));
                }

                log.LogInformation("Incoming message from Evaluate queue");
                var message = JsonConvert.DeserializeObject<QueueMessage<ServicePrincipalModel>>(msg.AsString);

                await _processor.Evaluate(context, message.Document).ConfigureAwait(false);

                context.End();
                log.LogInformation($"Evaluate Queue trigger function processed: {msg.Id} in {context.ElapsedTime}");
            }
            catch (Exception ex)
            {
                ex.Data["activityContext"] = context;
                log.LogError(ex, $"Message {msg.Id} aborting: {ex.Message}");
                throw;
            }

        }

        [FunctionName("UpdateAAD")]
        [StorageAccount(Constants.SPStorageConnectionString)]
        public async Task UpdateAAD([QueueTrigger(Constants.UpdateQueueAppSetting)] CloudQueueMessage msg, ILogger log)
        {
            var context = new ActivityContext("Update Service Principal");
            try
            {

                if (msg == null)
                {
                    throw new ArgumentNullException(nameof(msg));
                }

                log.LogInformation("Incoming message from Update queue");
                var message = JsonConvert.DeserializeObject<QueueMessage<ServicePrincipalUpdateCommand>>(msg.AsString);

                await _processor.UpdateServicePrincipal(context, message.Document).ConfigureAwait(false);

                context.End();
                log.LogInformation($"Update Queue trigger function processed: {msg.Id} in {context.ElapsedTime}");
            }
            catch (Exception ex)
            {
                ex.Data["activityContext"] = context;
                log.LogError(ex, $"Message {msg.Id} aborting: {ex.Message}");
                throw;
            }

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
