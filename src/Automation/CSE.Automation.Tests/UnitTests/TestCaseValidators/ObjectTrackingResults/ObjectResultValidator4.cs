using System;
using CSE.Automation.Model;
using static CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.ObjectTrackingResults
{
    internal class ObjectResultValidator4 : ObjectResultValidatorBase, IObjectResultValidator
    {

        public ObjectResultValidator4(TrackingModel savedObjectTracking, TrackingModel newObjectTracking, ActivityContext activityContext, TestCase testCase)
            : base(savedObjectTracking, newObjectTracking, activityContext, testCase)
        {
        }

        public override bool Validate()
        {

            bool objectExists = NewObjectTracking != null;


            bool validCorrelationIdPass = Guid.TryParse(NewObjectTracking.CorrelationId, out Guid dummyGuid) &&
                                        NewObjectTracking.CorrelationId.Equals(Context.CorrelationId);

            bool updatedPass = NewObjectTracking.LastUpdated > SavedObjectTracking.LastUpdated &&
                            NewObjectTracking.LastUpdated > NewObjectTracking.Created; //Different timestamp

            return (objectExists && validCorrelationIdPass && updatedPass);
        }
    }
}
