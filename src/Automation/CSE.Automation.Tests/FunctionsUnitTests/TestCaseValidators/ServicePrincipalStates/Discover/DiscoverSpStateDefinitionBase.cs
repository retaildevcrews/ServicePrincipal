using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates.Discover
{
    abstract class DiscoverSpStateDefinitionBase : IDiscoverSpStateDefinition
    {
        public TestCaseCollection.TestCase TestCaseID { get; }
        public string DisplayNamePatternFilter { get; }

        public DiscoverSpStateDefinitionBase(string displayNamePatternFilter, TestCase testCase)
        {
            TestCaseID = testCase;
            DisplayNamePatternFilter = displayNamePatternFilter;
        }

        abstract public bool Validate();
    }
}
