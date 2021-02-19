using Microsoft.Extensions.Configuration;
using static CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ServicePrincipalStates.Discover
{
    interface IDiscoverSpStateDefinition
    {
        TestCase TestCaseID { get; }
       
        bool Validate();

        IConfigurationRoot Config { get; }
    }
}
