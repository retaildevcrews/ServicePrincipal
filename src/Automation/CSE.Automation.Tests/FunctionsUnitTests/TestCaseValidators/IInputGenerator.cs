using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators
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
