using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CSE.Automation.Model;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates
{
    abstract class SpStateDefinitionBase : ISpStateDefinition
    {
        public ServicePrincipal ServicePrincipalObject { get; }

        public ServicePrincipalModel SPModel { get; private set; }

        public TestCase TestCaseID { get; }

        public SpStateDefinitionBase(ServicePrincipal servicePrincipal, TestCase testCase)
        {
            ServicePrincipalObject = servicePrincipal;
            TestCaseID = testCase;
        }

        public abstract ServicePrincipalWrapper Validate();
        
    }
}
