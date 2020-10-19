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

        public abstract int VisibilityDelayGapSeconds { get; }
        public abstract int QueueRecordProcessThreshold { get; }
        public abstract Guid ConfigurationId { get; }

        protected DeltaProcessorBase(IConfigService<ProcessorConfiguration> configService)
        {
            _configService = configService;
        }

        private protected void InitializeProcessor()
        {
            // Need the config for startup, so accepting the blocking call in the constructor.
            _config = _configService.GetConfig(this.ConfigurationId.ToString());
        }
        public abstract Task<int> ProcessDeltas();

    }
}
