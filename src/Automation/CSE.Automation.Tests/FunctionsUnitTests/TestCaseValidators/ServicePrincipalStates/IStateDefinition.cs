using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates
{
    interface IStateDefinition
    {
        TestCase TestCaseID { get; }
        ServicePrincipal ServicePrincipalObject { get; }

        ServicePrincipalWrapper Validate();
    }
}
