// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Extensions.Logging;

namespace CSE.Automation.Processors
{
    internal abstract class DeltaProcessorBase : IDeltaProcessor
    {
        protected readonly ILogger logger;
        protected readonly IConfigService<ProcessorConfiguration> configService;
        protected ProcessorConfiguration config;
        private bool initialized;

        protected DeltaProcessorBase(IConfigService<ProcessorConfiguration> configService, ILogger logger)
        {
            this.configService = configService;
            this.logger = logger;
        }

        public abstract int VisibilityDelayGapSeconds { get; }
        public abstract int QueueRecordProcessThreshold { get; }
        public abstract Guid ConfigurationId { get; }
        public abstract ProcessorType ProcessorType { get; }
        protected abstract string DefaultConfigurationResourceName { get; }

        public abstract Task RequestDiscovery(ActivityContext context, DiscoveryMode discoveryMode, string source);
        public abstract Task<IEnumerable<ActivityHistory>> GetActivityStatus(ActivityContext context, string activityId, string correlationId);
        public abstract Task<GraphOperationMetrics> DiscoverDeltas(ActivityContext context, bool forceReseed = false);

        public async Task Lock(string lockingActivityID)
        {
            await configService.Lock(this.ConfigurationId.ToString(), lockingActivityID, this.DefaultConfigurationResourceName).ConfigureAwait(false);
        }

        public async Task Unlock()
        {
            // Note: Integration test run before the system is up and running.
            // Discover queue integration tests create, utilize and dispose their own ProcessorConfiguration.
            // so Unlock method was also fixed to be able to unlock any configuration not just the default one that is hardcoded.
            await configService.Unlock(this.config.Id).ConfigureAwait(false);
        }

        protected void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            Initialize();
        }

        private void Initialize()
        {
            logger.LogInformation($"Initializing {this.GetType().Name}");

            config = configService.Get(this.ConfigurationId.ToString(), ProcessorType, DefaultConfigurationResourceName);
            if (config == null)
            {
                throw new ApplicationException($"Missing configuration for {ProcessorType}-{ConfigurationId}");
            }

            initialized = true;
        }
    }
}
