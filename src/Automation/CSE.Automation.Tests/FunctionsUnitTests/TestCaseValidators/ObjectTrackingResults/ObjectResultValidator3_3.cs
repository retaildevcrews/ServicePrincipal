using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ObjectTrackingResults
{
    internal class ObjectResultValidator3_2 : ObjectResultValidatorBase, IObjectResultValidator
    {

        public ObjectResultValidator3_2(TrackingModel savedObjectTracking, TrackingModel newObjectTracking, ActivityContext activityContext, TestCase testCase)
            : base(savedObjectTracking, newObjectTracking, activityContext, testCase)
        {
        }

        public override bool Validate()
        {

            bool objectExists = NewObjectTracking != null;


            bool validCorrelationId = Guid.TryParse(NewObjectTracking.CorrelationId, out Guid dummyGuid) &&
                                        NewObjectTracking.CorrelationId.Equals(Context.CorrelationId);

            bool created = NewObjectTracking.LastUpdated > SavedObjectTracking.LastUpdated &&
                            NewObjectTracking.LastUpdated == NewObjectTracking.Created;// same timestamp

            return (objectExists && validCorrelationId && created);
        }
    }
}
