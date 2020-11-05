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

        /// <summary>
        /// Request an AAD Discovery Activity
        /// </summary>
        /// <param name="req">An instance of an <see cref="HttpRequest"/>.</param>
        /// <param name="full">Flag to request a full scan.  Default is false.</param>
        /// <param name="log">An instance of an <see cref="ILogger"/>.</param>
        /// <returns>A JSON object containing information about the requested activity.</returns>
        [FunctionName("RequestDiscovery")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Ensure graceful return under all trappable error conditions.")]
        public async Task<IActionResult> RequestDiscovery([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req, ILogger log)
        {
            var discoveryMode = bool.TryParse(req.Query["full"], out var fullDiscovery) && fullDiscovery 
                                        ? DiscoveryMode.FullSeed
                                        : DiscoveryMode.Deltas;
            try
            {
                return req is null
                        ? throw new ArgumentNullException(nameof(req))
                        : await CommandDiscovery(discoveryMode, log).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var message = $"Failed to request Discovery {discoveryMode}";
                log.LogError(ex, message);

                return new BadRequestObjectResult(message);
            }
        }

        [FunctionName("DiscoverDeltas")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Ensure graceful return under all trappable error conditions.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Required as part of Trigger declaration.")]
        public async Task Deltas([TimerTrigger(Constants.DeltaDiscoverySchedule, RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            var discoveryMode = DiscoveryMode.Deltas;
            try
            {
                await CommandDiscovery(discoveryMode, log).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var message = $"Failed to request Discovery {discoveryMode}";
                log.LogError(ex, message);
            }
        }

        [FunctionName("Discover")]
        [StorageAccount(Constants.SPStorageConnectionString)]
        public async Task Discover([QueueTrigger(Constants.DiscoverQueueAppSetting)] CloudQueueMessage msg, ILogger log)
        {
            RequestDiscoveryCommand command;

            log.LogInformation("Incoming message from Discover queue");
            try
            {
                command = JsonConvert.DeserializeObject<QueueMessage<RequestDiscoveryCommand>>(msg.AsString).Document;
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Failed to deserialize queue message into RequestDicoveryCommand.");
                return;
            }

            using var context = activityService.CreateContext("Full Seed", withTracking: true, correlationId: command.CorrelationId);
            try
            {
                log.LogDebug("Executing Discover QueueTrigger Function");
                context.WithProcessorLock(processor);

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
            }
            catch (Exception ex)
            {
                context.Activity.Status = ActivityHistoryStatus.Failed;

                ex.Data["activityContext"] = context;
                log.LogError(ex, LockConflictMessage);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Ensure graceful return under all trappable error conditions.")]
        [FunctionName("Evaluate")]
        [StorageAccount(Constants.SPStorageConnectionString)]
        public async Task Evaluate([QueueTrigger(Constants.EvaluateQueueAppSetting)] CloudQueueMessage msg, ILogger log)
        {
            EvaluateServicePrincipalCommand command;

            log.LogInformation("Incoming message from Evaluate queue");
            try
            {
                command = JsonConvert.DeserializeObject<QueueMessage<EvaluateServicePrincipalCommand>>(msg.AsString).Document;
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Failed to deserialize queue message into EvaluateServicePrincipalCommand.");
                return;
            }

            using var context = activityService.CreateContext("Evaluate Service Principal", correlationId: command.CorrelationId);
            try
            {
                if (msg == null)
                {
                    throw new ArgumentNullException(nameof(msg));
                }

                await processor.Evaluate(context, command.Model).ConfigureAwait(false);

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Ensure graceful return under all trappable error conditions.")]
        [FunctionName("UpdateAAD")]
        [StorageAccount(Constants.SPStorageConnectionString)]
        public async Task UpdateAAD([QueueTrigger(Constants.UpdateQueueAppSetting)] CloudQueueMessage msg, ILogger log)
        {
            ServicePrincipalUpdateCommand command;

            log.LogTrace("Incoming message from Update queue");
            try
            {
                command = JsonConvert.DeserializeObject<QueueMessage<ServicePrincipalUpdateCommand>>(msg.AsString).Document;
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Failed to deserialize queue message into ServicePrincipalUpdateCommand.");
                return;
            }

            using var context = activityService.CreateContext("Update Service Principal", correlationId: command.CorrelationId);
            try
            {
                if (msg == null)
                {
                    throw new ArgumentNullException(nameof(msg));
                }

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

        /// <summary>
        /// Get the status of an activity.
        /// </summary>
        /// <param name="req">HttpRequest instance</param>
        /// <param name="correlationId">Id of the activity to report.</param>
        /// <param name="log">An instance of an <see cref="ILogger"/>.</param>
        /// <returns>A JSON object containing information about the requested activity.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Ensure graceful return under all trappable error conditions.")]
        [FunctionName("Activities")]
        public async Task<IActionResult> Activities([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, string correlationId, ILogger log)
        {
            using var context = activityService.CreateContext("Activities", withTracking: false).WithProcessorLock(processor);
            try
            {
                log.LogDebug("Executing ActivityStatus HttpTrigger Function");

                // TODO: If we end up with now request params needed for the seed function then remove the param and this check.
                if (req is null)
                {
                    throw new ArgumentNullException(nameof(req));
                }

                if (string.IsNullOrWhiteSpace(correlationId))
                {
                    throw new ArgumentNullException(nameof(correlationId));
                }

                var activityHistory = await processor.GetActivityStatus(context, correlationId).ConfigureAwait(false);
                context.End();

                var result = new
                {
                    Operation = "Activity Status",
                    Activity = activityHistory,
                    context.ElapsedTime,
                };

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                context.Activity.Status = ActivityHistoryStatus.Failed;

                ex.Data["activityContext"] = context;
                return new BadRequestObjectResult(context);
            }
        }

        private async Task<IActionResult> CommandDiscovery(DiscoveryMode discoveryMode, ILogger log)
        {
            using var context = activityService.CreateContext(discoveryMode == DiscoveryMode.FullSeed ? "Full Discovery" : "Delta Discovery", withTracking: false);
            try
            {
                await processor.RequestDiscovery(context, discoveryMode).ConfigureAwait(false);
                var result = new
                {
                    Timestamp = DateTimeOffset.Now,
                    Operation = discoveryMode.ToString(),
                    DiscoveryMode = discoveryMode,
                    ActivityId = context.Activity.Id,
                    CorrelationId = context.CorrelationId,
                };

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                context.Activity.Status = ActivityHistoryStatus.Failed;

                ex.Data["activityContext"] = context;

                var message = $"Failed to request Discovery {discoveryMode.ToString()}";
                log.LogError(ex, message);

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
