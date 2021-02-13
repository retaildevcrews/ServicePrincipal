using CSE.Automation.Model;
using static CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.ActivityResuts
{
    interface IActivityResultValidator
    {
        TestCase TestCaseID { get; }
        bool Validate();

        ActivityContext Context { get; }
    }
}
