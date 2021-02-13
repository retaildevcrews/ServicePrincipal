using Microsoft.Extensions.Configuration;
using static CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.ServicePrincipalStates.Discover
{
    interface IDiscoverSpStateDefinition
    {
        TestCase TestCaseID { get; }
       
        bool Validate();

        IConfigurationRoot Config { get; }
    }
}
