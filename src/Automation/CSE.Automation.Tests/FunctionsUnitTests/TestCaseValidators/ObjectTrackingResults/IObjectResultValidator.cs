using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ObjectTrackingResults
{
    interface IObjectResultValidator
    {
        TestCase TestCaseID { get; }
        bool Validate();

        ActivityContext Context { get; }
    }
}
