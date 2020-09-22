using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation.Processors
{
    public class DeltaProcessorBase : IDeltaProcessor
    {
        public string ProcessorId { get; private set; }         

        public DeltaProcessorBase (IDAL configDAL)
        {
            if (configDAL is null)
                throw new NullReferenceException("Null Configuration DAL passed to DeltaProcessor Constructor");

            var config = configDAL.GetById<Configuration>(ProcessorId);
        }

        public void ProcessDeltas()
        {
            throw new NotImplementedException();
        }
    }
}
