using Microsoft.Extensions.Configuration;
using static CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.ServicePrincipalStates.Update
{
    interface IUpdateSpStateDefinition
    {
        TestCase TestCaseID { get; }

        ServicePrincipalWrapper Validate();

        IConfigurationRoot Config { get; }

        string ServicePrincipalName { get; }
    }
}
