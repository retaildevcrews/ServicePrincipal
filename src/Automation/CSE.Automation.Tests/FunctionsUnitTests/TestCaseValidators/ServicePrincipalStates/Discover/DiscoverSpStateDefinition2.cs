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
    internal class DiscoverSpStateDefinition2 : DiscoverSpStateDefinitionBase, IDiscoverSpStateDefinition
    {
        public DiscoverSpStateDefinition2( string displayNamePatternFilter, TestCase testCase) : base(displayNamePatternFilter, testCase)
        {
        }
        public override bool Validate()
        {
            var servicePrincipalList = GraphHelper.GetAllServicePrincipals($"{DisplayNamePatternFilter}").Result;

            //GraphHelper.DeleteServicePrincipalsAsync("");

            // TODO : delete one of the SP in servicePrincipalList   

            return false;

        }
    }
}
