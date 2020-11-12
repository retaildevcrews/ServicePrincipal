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

        public TrackingModel SavedObjectTracking { get; }

        public TrackingModel NewObjectTracking { get;  }

        public ActivityContext Context { get; }

        public ObjectResultValidatorBase(TrackingModel savedObjectTracking, TrackingModel newObjectTracking, ActivityContext activityContext, TestCase testCase)
        {
            SavedObjectTracking = savedObjectTracking;
            NewObjectTracking = newObjectTracking;
            TestCaseID = testCase;
            Context = activityContext;
        }

        public abstract bool Validate();
    }
}
