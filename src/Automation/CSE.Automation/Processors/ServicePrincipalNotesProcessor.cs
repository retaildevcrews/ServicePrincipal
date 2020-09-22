using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Interfaces;
using CSE.Automation.Utilities;

namespace CSE.Automation.Processors
{
    public class ServicePrincipalNotesProcessor:DeltaProcessorBase
    {
        private readonly string _uniqueId = "E7575910-2F85-4373-8284-403E7D530C55";
        public ServicePrincipalNotesProcessor(IDAL configDAL):base(configDAL)
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
