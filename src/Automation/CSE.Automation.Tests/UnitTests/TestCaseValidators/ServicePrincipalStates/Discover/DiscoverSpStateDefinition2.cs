using System;
using System.Threading;
using CSE.Automation.Tests.UnitTests.TestCaseValidators.Helpers;
using CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases;
using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;
using Microsoft.Extensions.Configuration;
using static CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.ServicePrincipalStates.Discover
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
