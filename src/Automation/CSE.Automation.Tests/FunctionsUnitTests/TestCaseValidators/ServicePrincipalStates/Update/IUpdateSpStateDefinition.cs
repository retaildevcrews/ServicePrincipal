using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates.Update
{
    interface IUpdateSpStateDefinition
    {
        TestCase TestCaseID { get; }

        ServicePrincipalWrapper Validate();

        IConfigurationRoot Config { get; }

        string ServicePrincipalName { get; }
    }
}
