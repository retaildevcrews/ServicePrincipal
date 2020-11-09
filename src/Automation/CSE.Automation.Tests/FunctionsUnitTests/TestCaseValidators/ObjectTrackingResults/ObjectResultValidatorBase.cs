using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ObjectTrackingResults
{
    abstract class ObjectResultValidatorBase : IObjectResultValidator
    {
        public TestCase TestCaseID { get; }

        public ObjectResultValidatorBase(TrackingModel savedObjectTracking, TrackingModel newObjectTracking, TestCase testCase)
        {
            TestCaseID = testCase;
        }

        public abstract bool Validate();
    }
}
