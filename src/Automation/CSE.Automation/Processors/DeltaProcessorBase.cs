using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Utilities;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation.Processors
{
    public class DeltaProcessorBase : IDeltaProcessor
    {
        private IDAL _configDAL;
        private Configuration _config;
        private readonly string _uniqueId = string.Empty;
        public string ProcessorId
        {
            get
            {
                return _uniqueId;
            }
        }

        public DeltaProcessorBase (IDAL configDAL)
        {
            if (configDAL is null)
                throw new NullReferenceException("Null Configuration DAL passed to DeltaProcessor Constructor");

            if (string.IsNullOrWhiteSpace(ProcessorId))
                throw new NullReferenceException("Value not set for DeltaProcessor unique identifier");

            _configDAL = configDAL;

            // Need the config for startup, so accepting the blocking call in the constructor.
            _config = configDAL.GetById<Configuration>(ProcessorId).Result;

        }

        public void ProcessDeltas()
        {
            throw new NotImplementedException();
        }
    }
}
