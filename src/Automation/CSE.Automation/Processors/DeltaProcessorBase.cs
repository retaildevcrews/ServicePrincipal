using System;
using System.Configuration;
using System.Threading.Tasks;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
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
            if (ConfigurationId == Guid.Empty)
            {
                throw new ConfigurationErrorsException($"{GetType().Name}: ConfigurationId is invalid");
            }

            if (VisibilityDelayGapSeconds <= 0 || VisibilityDelayGapSeconds > Constants.MaxVisibilityDelayGapSeconds)
            {
                throw new ConfigurationErrorsException($"{GetType().Name}: VisibilityDelayGapSeconds is invalid");
            }

            if (QueueRecordProcessThreshold <= 0 || QueueRecordProcessThreshold > Constants.MaxQueueRecordProcessThreshold)
            {
                throw new ConfigurationErrorsException($"{GetType().Name}: QueueRecordProcessThreshold is invalid");
            }
        }
    }

    internal abstract class DeltaProcessorBase : IDeltaProcessor
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
            if (_initialized)
            {
                return;
            }

            Initialize();
        }

        private void Initialize()
        {
            _config = _configService.Get(ConfigurationId.ToString(), ProcessorType, DefaultConfigurationResource);
            _initialized = true;
        }

        public abstract Task<int> DiscoverDeltas(ActivityContext context, bool forceReseed = false);
    }
}
