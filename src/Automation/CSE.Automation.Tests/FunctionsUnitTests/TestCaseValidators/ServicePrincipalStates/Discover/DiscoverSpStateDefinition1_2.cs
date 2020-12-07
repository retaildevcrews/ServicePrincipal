using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates.Discover
{
    internal class DiscoverSpStateDefinition1_2 : DiscoverSpStateDefinitionBase, IDiscoverSpStateDefinition
    {
        public DiscoverSpStateDefinition1_2( string displayNamePatternFilter, TestCase testCase) : base(displayNamePatternFilter, testCase)
        {
        }
        public override bool Validate()
        {
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
