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

namespace CSE.Automation.Processors
{
    public class DeltaProcessorSettings : SettingsBase
    {
        public DeltaProcessorSettings(ISecretClient secretClient) : base(secretClient)
        {
        }
        public Guid ConfigurationId { get; set; }

        public int VisibilityDelayGapSeconds { get; set; }
        public int QueueRecordProcessThreshold { get; set; }

        public override void Validate()
        {
            base.Validate();
            if (this.ConfigurationId == Guid.Empty) throw new ConfigurationErrorsException($"{this.GetType().Name}: ConfigurationId is invalid");
            if (this.VisibilityDelayGapSeconds <= 0 || this.VisibilityDelayGapSeconds > Constants.MaxVisibilityDelayGapSeconds) throw new ConfigurationErrorsException($"{this.GetType().Name}: VisibilityDelayGapSeconds is invalid");
            if (this.QueueRecordProcessThreshold <= 0 || this.QueueRecordProcessThreshold > Constants.MaxQueueRecordProcessThreshold) throw new ConfigurationErrorsException($"{this.GetType().Name}: QueueRecordProcessThreshold is invalid");
        }
    }
    abstract class DeltaProcessorBase : IDeltaProcessor
    {
        protected readonly IConfigService<ProcessorConfiguration> _configService;

        protected ProcessorConfiguration _config;
        private bool _initialized;

        public abstract int VisibilityDelayGapSeconds { get; }
        public abstract int QueueRecordProcessThreshold { get; }
        public abstract Guid ConfigurationId { get; }
        public abstract ProcessorType ProcessorType { get; }
        protected abstract byte[] DefaultConfigurationResource { get; }

        protected DeltaProcessorBase(IConfigService<ProcessorConfiguration> configService)
        {
            _configService = configService;
        }

        protected void EnsureInitialized()
        {
            if (_initialized) return;
            Initialize();
        }

        private void Initialize()
        {
            _config = _configService.Get(this.ConfigurationId.ToString(), ProcessorType, DefaultConfigurationResource);
            _initialized = true;
        }

        public abstract Task<int> DiscoverDeltas(ActivityContext context, bool forceReseed = false);
    }
}
