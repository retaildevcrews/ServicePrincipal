using Microsoft.Graph;
using static CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ServicePrincipalResults
{
    interface ISpResultValidator
    {
        public string SavedServicePrincipalAsString { get; }

        public ServicePrincipal NewServicePrincipal { get; }

        TestCase TestCaseID { get; }
        bool Validate();
    }
}
