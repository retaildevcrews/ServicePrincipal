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
        protected readonly ICosmosDBRepository<ProcessorConfiguration> _configRepository;
        protected readonly ICosmosDBRepository<AuditEntry> _auditRepository;

        protected ProcessorConfiguration _config;
        private bool _initialized;

        public abstract int VisibilityDelayGapSeconds { get; }
        public abstract int QueueRecordProcessThreshold { get; }
        public abstract Guid ConfigurationId { get; }
        public abstract ProcessorType ProcessorType { get; }

        protected DeltaProcessorBase(ICosmosDBRepository<ProcessorConfiguration> configRepository, ICosmosDBRepository<AuditEntry> auditRepository)
        {
            _auditRepository = auditRepository;
            if (_auditRepository.Test().Result == false)
            {
                throw new ApplicationException($"Repository {_auditRepository.DatabaseName}:{_auditRepository.CollectionName} failed connection test");
            }

            _configRepository = configRepository;

        }

        protected void EnsureInitialized()
        {
            if (_initialized) return;
            Initialize();
        }

        private void Initialize()
        {
            // Need the config for startup, so accepting the blocking call in the constructor.
            if (_configRepository.Test().Result == false)
            {
                throw new ApplicationException($"Repository {_configRepository.DatabaseName}:{_configRepository.CollectionName} failed connection test during {this.GetType().Name}::ctor");
            }
            _config = GetConfigDocumentOrCreateInitialDocumentIfDoesNotExist();
            _initialized = true;
        }

        protected abstract byte[] DefaultConfigurationResource { get; }

        private ProcessorConfiguration GetConfigDocumentOrCreateInitialDocumentIfDoesNotExist()
        {
            ProcessorConfiguration configuration = _configRepository.GetByIdAsync(ConfigurationId.ToString(), ProcessorType.ToString()).GetAwaiter().GetResult();
            if (configuration == null)
            {
                var defaultConfig = DefaultConfigurationResource;
                if (defaultConfig == null || defaultConfig.Length == 0)
                {
                    throw new NullReferenceException("Null or empty initial Configuration Document resource.");
                }

                var initalDocumentAsString = System.Text.Encoding.Default.GetString(defaultConfig);

                try
                {
                    ProcessorConfiguration defaultConfiguration = JsonConvert.DeserializeObject<ProcessorConfiguration>(initalDocumentAsString);
                    return _configRepository.CreateDocumentAsync(defaultConfiguration).Result;
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException("Unable to deserialize Initial Configuration Document.", ex);
                }
            }

            return configuration;

        }

        public abstract Task<int> DiscoverDeltas(ActivityContext context);

    }
}
