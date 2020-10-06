using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Graph;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.Design;

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
            _config = _repository.GetById<ProcessorConfiguration>(_uniqueId, processorType.ToString()).Result;
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

        public void ProcessDeltas()
        {
            throw new NotImplementedException();
        }
    }
}
