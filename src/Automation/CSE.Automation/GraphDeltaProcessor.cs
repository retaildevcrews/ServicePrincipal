// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Processors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CSE.Automation
{
    internal class GraphDeltaProcessor
    {
        private readonly IActivityService activityService;
        private readonly IServicePrincipalProcessor processor;
        private readonly ILogger logger;

        // TODO: move to resource
        private const string LockConflictMessage = "Processor lock conflict";

        public GraphDeltaProcessor(IServiceProvider serviceProvider, IActivityService activityService, IServicePrincipalProcessor processor, ILogger<GraphDeltaProcessor> logger)
        {
            this.activityService = activityService;
            this.processor = processor;
            this.logger = logger;

            ValidateServices(serviceProvider);
        }

        [FunctionName("DiscoverDeltas")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Ensure graceful return under all trappable error conditions.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Required as part of Trigger declaration.")]
        public async Task Deltas([TimerTrigger(Constants.DeltaDiscoverySchedule, RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            using var context = activityService.CreateContext("Delta Detection", withTracking: true).WithProcessorLock(processor);
            try
            {
                log.LogDebug("Executing SeedDeltaProcessorTimer Function");

                var metrics = await processor.DiscoverDeltas(context, false).ConfigureAwait(false);
                context.End();

                log.LogTrace($"Deltas: {metrics.Found} ServicePrincipals discovered in {context.ElapsedTime}.");
            }
            catch (Exception ex)
            {
                context.Activity.Status = ActivityHistoryStatus.Failed;

                ex.Data["activityContext"] = context;
                log.LogError(ex, LockConflictMessage);
            }
        }

        [FunctionName("FullSeed")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Ensure graceful return under all trappable error conditions.")]
        public async Task<IActionResult> FullSeed([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            using var context = activityService.CreateContext("Full Seed", withTracking: true).WithProcessorLock(processor);
            try
            {
                log.LogDebug("Executing SeedDeltaProcessor HttpTrigger Function");

                // TODO: If we end up with now request params needed for the seed function then remove the param and this check.
                if (req is null)
                {
                    throw new ArgumentNullException(nameof(req));
                }

                var metrics = await processor.DiscoverDeltas(context, true).ConfigureAwait(false);
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
            catch (Exception ex)
            {
                context.Activity.Status = ActivityHistoryStatus.Failed;

                ex.Data["activityContext"] = context;
                log.LogError(ex, LockConflictMessage);
                return new BadRequestObjectResult($"Cannot start processor: {LockConflictMessage}");
            }
        }

        [FunctionName("Evaluate")]
        [StorageAccount(Constants.SPStorageConnectionString)]
        public async Task Evaluate([QueueTrigger(Constants.EvaluateQueueAppSetting)] CloudQueueMessage msg, ILogger log)
        {
            using var context = activityService.CreateContext("Evaluate Service Principal");
            try
            {
                if (msg == null)
                {
                    throw new ArgumentNullException(nameof(msg));
                }

                log.LogTrace("Incoming message from Evaluate queue");
                var message = JsonConvert.DeserializeObject<QueueMessage<ServicePrincipalModel>>(msg.AsString);

                await processor.Evaluate(context, message.Document).ConfigureAwait(false);

                context.End();
                log.LogTrace($"Evaluate Queue trigger function processed: {msg.Id} in {context.ElapsedTime}");
            }
            catch (Exception ex)
            {
                context.Activity.Status = ActivityHistoryStatus.Failed;

                ex.Data["activityContext"] = context;
                log.LogError(ex, $"Message {msg.Id} aborting: {ex.Message}");
                throw;
            }
        }

        [FunctionName("UpdateAAD")]
        [StorageAccount(Constants.SPStorageConnectionString)]
        public async Task UpdateAAD([QueueTrigger(Constants.UpdateQueueAppSetting)] CloudQueueMessage msg, ILogger log)
        {
            using var context = activityService.CreateContext("Update Service Principal");
            try
            {
                if (msg == null)
                {
                    throw new ArgumentNullException(nameof(msg));
                }

                log.LogTrace("Incoming message from Update queue");
                var message = JsonConvert.DeserializeObject<QueueMessage<ServicePrincipalUpdateCommand>>(msg.AsString);

                await processor.UpdateServicePrincipal(context, message.Document).ConfigureAwait(false);

                context.End();
                log.LogTrace($"Update Queue trigger function processed: {msg.Id} in {context.ElapsedTime}");
            }
            catch (Exception ex)
            {
                context.Activity.Status = ActivityHistoryStatus.Failed;

                ex.Data["activityContext"] = context;
                log.LogError(ex, $"Message {msg.Id} aborting: {ex.Message}");
                throw;
            }
        }

        private void ValidateServices(IServiceProvider serviceProvider)
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
                    logger.LogInformation(message);
                }
                else
                {
                    logger.LogError(message);
                }
            }

            if (hasFailingTest)
            {
                throw new ApplicationException($"One or more repositories failed test.");
            }
        }
    }
}
