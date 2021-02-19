using System.IO;
using System.Linq;
using CSE.Automation.Tests.IntegrationTests.TestCaseValidators.Helpers;
using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;
using Microsoft.Extensions.Configuration;
using static CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ServicePrincipalStates.Discover
{
    internal class DiscoverSpStateDefinition1 : DiscoverSpStateDefinitionBase, IDiscoverSpStateDefinition
    {
        public DiscoverSpStateDefinition1(IConfigurationRoot config, TestCase testCase, GraphDeltaProcessorHelper graphDeltaProcessorHelper) : base(config, testCase, graphDeltaProcessorHelper)
        {
        }
        public override bool Validate()
        {
            if (GraphDeltaProcessorHelper != null && GraphDeltaProcessorHelper.DeleteDynamicCreatedServicePrincipals)
            {
                DeleteDynamicCreatedTestServicePrincipals();
            }
            else if (GraphDeltaProcessorHelper == null)
            {
                DeleteDynamicCreatedTestServicePrincipals();
            }

            var servicePrincipalList = GraphHelper.GetAllServicePrincipals($"{DisplayNamePatternFilter}").Result;

            if (servicePrincipalList.Count() > 0)
            {
                return true;
            }
            else
            {
                throw new InvalidDataException($"Unable to find any AAD Service Principal that match the search pattern [{DisplayNamePatternFilter}] does not match Test Case [{TestCaseID}] rules.");
            }
            
        }
    }
}
