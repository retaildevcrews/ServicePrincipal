using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates
{
    interface ISpStateDefinition
    {
        TestCase TestCaseID { get; }
        ServicePrincipal ServicePrincipalObject { get; }

        ServicePrincipalModel SPModel { get; }

        ServicePrincipalWrapper Validate();

        ServicePrincipalWrapper GetNewServicePrincipalWrapper();
    }
}
