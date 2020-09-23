using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Interfaces;
using CSE.Automation.Utilities;

namespace CSE.Automation.Processors
{
    public class ServicePrincipalProcessor:DeltaProcessorBase
    {
        public ServicePrincipalProcessor(IDAL configDAL):base(configDAL)
        {

            // TODO:
            // 1. Create sample config and add to CosmosDB
            // 2. Ensure config loads and parses properly
            // 3. Write test to construct ServicrPrincipalNoateProcessor directly
            // 4. Construct graph helper with config information
            // 5. Refactor GraphHelper classes to use configuration information
        }
        public void ProcessDeltas()
        {

        }
    }
}
