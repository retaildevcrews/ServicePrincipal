using CSE.Automation.Model;
using static CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.ObjectTrackingResults
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
