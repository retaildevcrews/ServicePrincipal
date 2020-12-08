using System;
using System.Collections.Generic;
using System.Text;
using AzQueueTestTool.TestCases.ServicePrincipals;
using CSE.Automation.Model;
using Microsoft.Graph;
using Newtonsoft.Json;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalResults.Discover
{
    internal class DiscoverSpResultValidator2 : SpResultValidatorBase, ISpResultValidator
    {

        public DiscoverSpResultValidator2(string savedServicePrincipalAsString, IInputGenerator inputGenerator, ActivityContext activityContext) 
                                    : base(savedServicePrincipalAsString, inputGenerator, activityContext, false)
        {
        }

        public override bool Validate()
        {
            // Cleanup delete ServicePrincipal Created from DiscoverSpStateDefinition2.cs

            string servicePrincipalToDelete = $"{DisplayNamePatternFilter}-REMOVED";

            var servicePrincipalList = GraphHelper.GetAllServicePrincipals(servicePrincipalToDelete).Result;

            if (servicePrincipalList.Count > 0)
            {
                GraphHelper.DeleteServicePrincipalsAsync(servicePrincipalList); 
            }

            // Validation 
            // We check for messages in Evaluate queue for Discover Test cases.
            int messageFoundCount = GetMessageCountInEvaluateQueueFor(this.DisplayNamePatternFilter);
            return messageFoundCount == 0;

        }
    }
}
