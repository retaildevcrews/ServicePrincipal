using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzQueueTestTool.TestCases.ServicePrincipals;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.Helpers;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates.Discover
{
    internal class DiscoverSpStateDefinition2 : DiscoverSpStateDefinitionBase, IDiscoverSpStateDefinition
    {
        public DiscoverSpStateDefinition2(IConfigurationRoot config, TestCase testCase, GraphDeltaProcessorHelper graphDeltaProcessorHelper) : base(config, testCase, graphDeltaProcessorHelper)
        {
        }
        public override bool Validate()
        {
            try
            {
                Thread.Sleep(5000);// when running all Test Cases we need to introduce some latency before it

                string servicePrincipalToDelete = $"{DisplayNamePatternFilter}{TestCaseCollection.TestRemovedAttributeSuffix}";

                var servicePrincipalList = GraphHelper.GetAllServicePrincipals(servicePrincipalToDelete).Result;

                if (servicePrincipalList.Count == 0)
                {
                    GraphHelper.CreateServicePrincipal(servicePrincipalToDelete);

                    servicePrincipalList = GraphHelper.GetAllServicePrincipals(servicePrincipalToDelete).Result;

                }

                //NOTE: this SP object will be deleted downstream from ServicePrincipalGraphHelperTest after filter string is generated
                
                return servicePrincipalList.Count == 1; // Test Service Principal must exists
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to validate precondition for Discover - Test Case [{TestCaseID}]", ex);
            }
            
        }
    }
}
