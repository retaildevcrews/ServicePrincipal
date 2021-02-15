using CSE.Automation.Model;
using static CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ObjectTrackingResults
{
    interface IObjectResultValidator
    {
        TestCase TestCaseID { get; }
        bool Validate();

        ActivityContext Context { get; }
    }
}
