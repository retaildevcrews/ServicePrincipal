﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CSE.Automation.Extensions;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Processors;
using CSE.Automation.Properties;
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
            var hasRedirect = req.Query.ContainsKey("redirect");

            try
            {
                if (req is null)
                {
                    throw new ArgumentNullException(nameof(req));
                }

                var result = await CommandDiscovery(discoveryMode, "HTTP", log).ConfigureAwait(false);

                return hasRedirect
                        // TODO: construct this URI properly
                        ? new RedirectResult($"{req.Scheme}://{req.Host}/api/Activities?correlationId={result.CorrelationId}")
                        : (IActionResult)new JsonResult(result);

            }
            catch (Exception ex)
            {
                var message = $"Failed to request Discovery {discoveryMode}";
                log.LogError(ex, message);

                return new BadRequestObjectResult(message);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Ensure graceful return under all trappable error conditions.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Required as part of Trigger declaration.")]
        [FunctionName("DiscoverDeltas")]
        public async Task Deltas([TimerTrigger(Constants.DeltaDiscoverySchedule, RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            var discoveryMode = DiscoveryMode.Deltas;
            try
            {
                await CommandDiscovery(discoveryMode, "TIMER", log).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var message = $"Failed to request Discovery {discoveryMode}";
                log.LogError(ex, message);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Ensure graceful return under all trappable error conditions.")]
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

            var operation = command.DiscoveryMode.Description();
            using var context = activityService.CreateContext(operation, withTracking: true, correlationId: command.CorrelationId);
            try
            {
                log.LogDebug("Executing Discover QueueTrigger Function");

                context.Activity.CommandSource = command.Source;
                context.WithProcessorLock(processor);

                var metrics = await processor.DiscoverDeltas(context, true).ConfigureAwait(false);
                context.End();
            }
            catch (Exception ex)
            {
                context.Activity.Status = ActivityHistoryStatus.Failed;

                ex.Data["activityContext"] = context;
                log.LogError(ex, Resources.LockConflictMessage);
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
                context.Activity.CommandSource = "QUEUE";

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
                context.Activity.CommandSource = "QUEUE";

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
        /// <param name="log">An instance of an <see cref="ILogger"/>.</param>
        /// <returns>A JSON object containing information about the requested activity.</returns>
        /// <remarks>Querystring parameters: activityId, correlationId.  One of the two must be provided.</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Ensure graceful return under all trappable error conditions.")]
        [FunctionName("Activities")]
        public async Task<IActionResult> Activities([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            var activityId = req.Query["activityId"];
            var correlationId = req.Query["correlationId"];

            using var context = activityService.CreateContext("Activities", withTracking: false);
            try
            {
                log.LogDebug("Executing ActivityStatus HttpTrigger Function");

                if (string.IsNullOrWhiteSpace(correlationId) && string.IsNullOrWhiteSpace(activityId))
                {
                    throw new InvalidOperationException("Either activityId or correlationId must be specified.");
                }

                var activityHistory = await processor.GetActivityStatus(context, activityId, correlationId).ConfigureAwait(false);
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
                log.LogError(ex, "Failed to retrieve activity status.");

                return new BadRequestObjectResult(ex.Message);
            }
        }

        private async Task<dynamic> CommandDiscovery(DiscoveryMode discoveryMode, string source, ILogger log)
        {
            using var context = activityService.CreateContext($"{discoveryMode.Description()} Request", withTracking: true);
            try
            {
                context.Activity.CommandSource = source;
                await processor.RequestDiscovery(context, discoveryMode, source).ConfigureAwait(false);
                var result = new
                {
                    Timestamp = DateTimeOffset.Now,
                    Operation = discoveryMode.ToString(),
                    DiscoveryMode = discoveryMode,
                    ActivityId = context.Activity.Id,
                    CorrelationId = context.CorrelationId,
                };

                return result;
            }
            catch (Exception ex)
            {
                context.Activity.Status = ActivityHistoryStatus.Failed;

                ex.Data["activityContext"] = context;

                var message = $"Failed to request Discovery {discoveryMode}";
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
