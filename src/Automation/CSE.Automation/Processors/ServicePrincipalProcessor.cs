using CSE.Automation.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation.Processors
{
    public class ServicePrincipalProcessor:DeltaProcessorBase
    {
        

        public ServicePrincipalProcessor(IDAL configDAL) : base(configDAL)
        {
            _uniqueId = "02a54ac9-441e-43f1-88ee-fde420db2559";
            InitializeProcessor(ProcessorType.ServicePrincipal);
        }


    }
}
