using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Properties;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
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
            if (this.VisibilityDelayGapSeconds <= 0 || this.VisibilityDelayGapSeconds > 500) throw new ConfigurationErrorsException($"{this.GetType().Name}: VisibilityDelayGapSeconds is invalid");
            if (this.QueueRecordProcessThreshold <= 0 || this.QueueRecordProcessThreshold > 3000) throw new ConfigurationErrorsException($"{this.GetType().Name}: QueueRecordProcessThreshold is invalid");
        }
    }
    abstract class DeltaProcessorBase : IDeltaProcessor
    {
        protected readonly ICosmosDBRepository _repository;
        protected ProcessorConfiguration _config;

        public abstract int VisibilityDelayGapSeconds { get; }
        public abstract int QueueRecordProcessThreshold { get; }
        public abstract Guid ConfigurationId { get; }

        protected DeltaProcessorBase (ICosmosDBRepository repository)
        {
            //if (configDAL is null)
            //    throw new NullReferenceException("Null Configuration DAL passed to DeltaProcessor Constructor");

            //_configDAL = configDAL;
            _repository = repository;
            if (_repository.Test().Result == false)
            {
                throw new ApplicationException($"Repository {_repository.DatabaseName}:{_repository.CollectionName} failed connection test");
            }
        }

        private protected void InitializeProcessor()
        {
            // Need the config for startup, so accepting the blocking call in the constructor.

           _config = GetConfigDocumentOrCreateInitialDocumentIfDoesNotExist();
        }

        private ProcessorConfiguration GetConfigDocumentOrCreateInitialDocumentIfDoesNotExist()
        {
            
            if (!_repository.DoesExistsAsync(this.ConfigurationId.ToString()).Result)
            {

                if (Resources.InitialProcessorConfigurationDocument == null || Resources.InitialProcessorConfigurationDocument.Length == 0)
                    throw new NullReferenceException("Null or empty initial Configuration Document resource.");
                
                var initalDocumentAsString = System.Text.Encoding.Default.GetString(Resources.InitialProcessorConfigurationDocument);

                try
                {
                    ProcessorConfiguration initialConfigDocumentAsJson = JsonConvert.DeserializeObject<ProcessorConfiguration>(initalDocumentAsString);
                    return _repository.CreateDocumentAsync<ProcessorConfiguration>(initialConfigDocumentAsJson, _repository.ResolvePartitionKey(initialConfigDocumentAsJson.Id)).Result;
                }
                catch(Exception ex)
                {
                    throw new InvalidDataException("Unable to deserialize Initial Configuration Document.", ex);
                }
            }
            else
            {
                return _repository.GetByIdAsync<ProcessorConfiguration>(this.ConfigurationId.ToString()).Result;
            }

        }

        public abstract Task<int> ProcessDeltas();

    }
}
