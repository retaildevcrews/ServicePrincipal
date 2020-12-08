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
    internal class DiscoverSpResultValidator1_2 : SpResultValidatorBase, ISpResultValidator
    {

        public DiscoverSpResultValidator1_2(string savedServicePrincipalAsString, IInputGenerator inputGenerator, ActivityContext activityContext) 
                                    : base(savedServicePrincipalAsString, inputGenerator, activityContext, false)
        {
        }

        public override bool Validate()
        {
            // We check for messages in Evaluate queue for Discover Test cases.
            int messageFoundCount = GetMessageCountInEvaluateQueueFor(this.DisplayNamePatternFilter);
            return messageFoundCount == 0;

        }
    }
}
