using CSE.Automation.Model;
using CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases;
using Microsoft.Graph;
using static CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators
{
    internal interface IInputGenerator
    {
        TestCase TestCaseId { get; }
        ITestCaseCollection TestCaseCollection { get; }
        ServicePrincipalModel GetServicePrincipalModel();
        ServicePrincipal GetServicePrincipal(bool requery = false);
        string StorageConnectionString { get; }
        string UpdateQueueName { get; }
        string EvaluateQueueName { get; }
        string AadUserServicePrincipalPrefix { get; }
        string DisplayNamePatternFilter { get; }

        string ConfigId { get; }
    }
}
