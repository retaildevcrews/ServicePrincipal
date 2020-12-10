using System;
using System.Collections.Generic;
using System.Text;
using AzQueueTestTool.TestCases.ServicePrincipals;
using CSE.Automation.Model;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases;
using Microsoft.Graph;
using Newtonsoft.Json;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalResults
{
    internal class DiscoverSpResultValidator3 : SpResultValidatorBase, ISpResultValidator
    {

        public DiscoverSpResultValidator3(string savedServicePrincipalAsString, IInputGenerator inputGenerator, ActivityContext activityContext) 
                                    : base(savedServicePrincipalAsString, inputGenerator, activityContext, false)
        {
        }

        public override bool Validate()
        {
            // Cleanup delete ServicePrincipal Created from DiscoverSpStateDefinition2.cs

            string servicePrincipalToDelete = $"{DisplayNamePatternFilter}{TestCaseCollection.TestNewUserSuffix}";

            DeleteServicePrincipal(servicePrincipalToDelete);

            string servicePrincipalId = TestCaseCollection.ServicePrincipalIdForTestNewUser;
            // We are checking for the SPECIFIC message for the SP exists in the Queue 
            
            bool messageFound = DoesMessageExistInEvaluateQueue(servicePrincipalId);

            TestCaseCollection.ServicePrincipalIdForTestNewUser = string.Empty;

            return messageFound;
        }
    }
}
