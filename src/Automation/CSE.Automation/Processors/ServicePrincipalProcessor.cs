using CSE.Automation.Interfaces;

using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.DataAccess;

namespace CSE.Automation.Processors
{
    public interface IServicePrincipalProcessor {}

    class ServicePrincipalProcessor : DeltaProcessorBase, IServicePrincipalProcessor
    {
        public ServicePrincipalProcessor(IConfigRepository repository) : base((ICosmosDBRepository)repository)
        {
            _uniqueId = "02a54ac9-441e-43f1-88ee-fde420db2559";
            InitializeProcessor(ProcessorType.ServicePrincipal);
        }


    }
}
