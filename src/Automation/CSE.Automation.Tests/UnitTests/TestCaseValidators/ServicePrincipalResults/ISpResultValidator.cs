using Microsoft.Graph;
using static CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.ServicePrincipalResults
{
    interface ISpResultValidator
    {
        public string SavedServicePrincipalAsString { get; }

        public ServicePrincipal NewServicePrincipal { get; }

        TestCase TestCaseID { get; }
        bool Validate();
    }
}
