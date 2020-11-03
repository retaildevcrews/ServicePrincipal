using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Properties;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using SettingsBase = CSE.Automation.Model.SettingsBase;
using Microsoft.Identity.Client;
using System.Net.WebSockets;
using CSE.Automation.Graph;
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

        public abstract Task<GraphOperationMetrics> DiscoverDeltas(ActivityContext context, bool forceReseed = false);

        public async Task Lock()
        {
            await configService.Lock(this.ConfigurationId.ToString(), this.DefaultConfigurationResourceName).ConfigureAwait(false);
        }

        public async Task Unlock()
        {
            await configService.Unlock().ConfigureAwait(false);
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

            config = configService.Get(this.ConfigurationId.ToString(), ProcessorType, DefaultConfigurationResourceName, true);
            initialized = true;
        }
    }
}
