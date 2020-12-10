using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates.Discover
{
    internal class DiscoverSpStateDefinition2 : DiscoverSpStateDefinitionBase, IDiscoverSpStateDefinition
    {
        public DiscoverSpStateDefinition2( string displayNamePatternFilter, TestCase testCase) : base(displayNamePatternFilter, testCase)
        {
        }
        public override bool Validate()
        {
            try
            {
                string servicePrincipalToDelete = $"{DisplayNamePatternFilter}-TEST_REMOVED_ATTRIBUTE";

                var servicePrincipalList = GraphHelper.GetAllServicePrincipals(servicePrincipalToDelete).Result;

                if (servicePrincipalList.Count == 0)
                {
                    GraphHelper.CreateServiceThisPrincipal(servicePrincipalToDelete);

                    servicePrincipalList = GraphHelper.GetAllServicePrincipals(servicePrincipalToDelete).Result;

                }

                return servicePrincipalList.Count == 1;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to validate precondition for Discover - Test Case [{TestCaseID}]", ex);
            }
            
        }
    }
}
