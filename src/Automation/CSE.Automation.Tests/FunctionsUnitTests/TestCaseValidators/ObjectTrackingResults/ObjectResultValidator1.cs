using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ObjectTrackingResults
{
    internal class ObjectResultValidator1 : ObjectResultValidatorBase, IObjectResultValidator
    {

        public ObjectResultValidator1(TrackingModel savedObjectTracking, TrackingModel newObjectTracking, ActivityContext activityContext, TestCase testCase)
            : base(savedObjectTracking, newObjectTracking, activityContext, testCase)
        {
        }

        public override bool Validate()
        {

            bool objectExistsPass = NewObjectTracking != null;


            bool validCorrelationIdPass = Guid.TryParse(NewObjectTracking.CorrelationId, out Guid dummyGuid) &&
                                        NewObjectTracking.CorrelationId.Equals(Context.CorrelationId);

            bool createdPass = NewObjectTracking.LastUpdated > SavedObjectTracking.LastUpdated &&
                            NewObjectTracking.LastUpdated == NewObjectTracking.Created;// same timestamp

            return (objectExistsPass && validCorrelationIdPass && createdPass);
        }
    }
}
