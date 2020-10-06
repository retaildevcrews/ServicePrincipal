using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Properties;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CSE.Automation.Processors
{
    public class DeltaProcessorBase : IDeltaProcessor
    {
        protected readonly ICosmosDBRepository _repository;
        protected ProcessorConfiguration _config;
        protected string _uniqueId = string.Empty;
        public string ProcessorId
        {
            get
            {
                return _uniqueId;
            }
        }

        private protected void InitializeProcessor(ProcessorType processorType)
        {
            // Need the config for startup, so accepting the blocking call in the constructor.

           _config = GetConfigDocumentOrCreateInitialDocumentIfDoesNotExist(processorType);
        }

        private protected ProcessorConfiguration GetConfigDocumentOrCreateInitialDocumentIfDoesNotExist(ProcessorType processorType)
        {
            
            if (!_configDAL.DoesExistsAsync(_uniqueId, processorType.ToString()).Result)
            {

                if (Resources.InitialProcessorConfigurationDocument == null || Resources.InitialProcessorConfigurationDocument.Length == 0)
                    throw new NullReferenceException("Null or empty initial Configuration Document resource.");
                
                var initalDocumentAsString = System.Text.Encoding.Default.GetString(Resources.InitialProcessorConfigurationDocument);

                try
                {
                    ProcessorConfiguration initialConfigDocumentAsJson = JsonConvert.DeserializeObject<ProcessorConfiguration>(initalDocumentAsString);
                    return _configDAL.CreateDocumentAsync<ProcessorConfiguration>(initialConfigDocumentAsJson, processorType.ToString()).Result;
                }
                catch(Exception ex)
                {
                    throw new InvalidDataException("Unable to deserialize Initial Configuration Document.", ex);
                }
            }
            else
            {
                return _configDAL.GetByIdAsync<ProcessorConfiguration>(_uniqueId, processorType.ToString()).Result;
            }

        }

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

        public virtual Task<int> ProcessDeltas()
        {
            throw new NotImplementedException();
        }
    }
}
