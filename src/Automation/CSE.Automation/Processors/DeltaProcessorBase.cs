using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using System;

using System.Threading.Tasks;

namespace CSE.Automation.Processors
{
    public class DeltaProcessorBase : IDeltaProcessor
    {
        protected IDAL _configDAL;
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
            _config = _configDAL.GetById<ProcessorConfiguration>(_uniqueId, processorType.ToString()).Result;
        }

        public DeltaProcessorBase (IDAL configDAL)
        {
            if (configDAL is null)
                throw new NullReferenceException("Null Configuration DAL passed to DeltaProcessor Constructor");

            _configDAL = configDAL;
        }

        public virtual Task ProcessDeltas()
        {
            throw new NotImplementedException();
        }
    }
}
