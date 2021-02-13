using CSE.Automation.Model;
using static CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.AuditResults
{
    interface IAuditResultValidator
    {
        TestCase TestCaseID { get; }
        bool Validate();

        ActivityContext Context { get; }
    }
}
