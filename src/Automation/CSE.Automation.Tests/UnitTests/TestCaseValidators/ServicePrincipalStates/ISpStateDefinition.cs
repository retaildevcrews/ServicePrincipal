using CSE.Automation.Model;
using Microsoft.Graph;
using static CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.ServicePrincipalStates
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
