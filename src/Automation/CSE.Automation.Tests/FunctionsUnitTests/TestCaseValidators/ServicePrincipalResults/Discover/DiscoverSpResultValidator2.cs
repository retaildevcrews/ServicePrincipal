﻿using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases;
using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;
using Microsoft.Graph;
using Newtonsoft.Json;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalResults
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

            string servicePrincipalToDelete = $"{DisplayNamePatternFilter}{TestCaseCollection.TestRemovedAttributeSuffix}";

            DeleteServicePrincipal(servicePrincipalToDelete);

            // The Max number of SPs queried by Delta request for testing purposes is 100. See ServicePrincipalGraphHelperTest.GetFilterString
            // So we will only try to get up to 100 SPs for a given Prefix
            var servicePrincipalList = GraphHelper.GetAllServicePrincipals($"{this.DisplayNamePatternFilter}", 100).Result;

            // We check for messages in Evaluate queue for Discover Test cases.
            int messageFoundCount = GetMessageCountInEvaluateQueueFor(this.DisplayNamePatternFilter);

            return servicePrincipalList.Count == messageFoundCount;// Messages must exist because it was executed as FullSeed run
        }
    }
}
