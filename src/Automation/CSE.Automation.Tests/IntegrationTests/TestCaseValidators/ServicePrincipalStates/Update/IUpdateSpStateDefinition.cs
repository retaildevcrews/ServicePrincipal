using Microsoft.Extensions.Configuration;
using static CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ServicePrincipalStates.Update
{
    interface IUpdateSpStateDefinition
    {
        TestCase TestCaseID { get; }

        ServicePrincipalWrapper Validate();

        IConfigurationRoot Config { get; }

        string ServicePrincipalName { get; }
    }
}
