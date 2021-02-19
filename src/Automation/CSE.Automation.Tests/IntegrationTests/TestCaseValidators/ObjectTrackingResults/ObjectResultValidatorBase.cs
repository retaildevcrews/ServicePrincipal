using CSE.Automation.Model;
using static CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ObjectTrackingResults
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
