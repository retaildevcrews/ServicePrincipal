using CSE.Automation.Model;
using static CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ObjectTrackingResults
{
    internal class ObjectResultValidator5 : ObjectResultValidatorBase, IObjectResultValidator
    {

        public ObjectResultValidator5(TrackingModel savedObjectTracking, TrackingModel newObjectTracking, ActivityContext activityContext, TestCase testCase)
            : base(savedObjectTracking, newObjectTracking, activityContext, testCase)
        {
        }

        public override bool Validate()
        {

            bool objectDoesNotExistsPass = NewObjectTracking == null;

            return (objectDoesNotExistsPass);
        }
    }
}
