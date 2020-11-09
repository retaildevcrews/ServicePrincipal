using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates
{
    abstract class StateDefinitionBase : IStateDefinition
    {
        public ServicePrincipal ServicePrincipalObject { get; }

        public TestCase TestCaseID { get; }

        public StateDefinitionBase(ServicePrincipal servicePrincipal, TestCase testCase)
        {
            ServicePrincipalObject = servicePrincipal;
            TestCaseID = testCase;
        }

        public abstract ServicePrincipalWrapper Validate();
    }
}
