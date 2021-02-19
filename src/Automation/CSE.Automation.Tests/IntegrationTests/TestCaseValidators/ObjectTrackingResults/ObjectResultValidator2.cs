using System;
using CSE.Automation.Model;
using static CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ObjectTrackingResults
{
    internal class ObjectResultValidator2 : ObjectResultValidatorBase, IObjectResultValidator
    {

        public ObjectResultValidator2(TrackingModel savedObjectTracking, TrackingModel newObjectTracking, ActivityContext activityContext, TestCase testCase)
            : base(savedObjectTracking, newObjectTracking, activityContext, testCase)
        {
        }

        public override bool Validate()
        {

            bool objectExistsPass = NewObjectTracking != null;


            bool validCorrelationIdPass = Guid.TryParse(NewObjectTracking.CorrelationId, out Guid dummyGuid) &&
                                        NewObjectTracking.CorrelationId.Equals(Context.CorrelationId);

            bool createdPass = NewObjectTracking.LastUpdated == NewObjectTracking.Created;// same timestamp

            return (objectExistsPass && validCorrelationIdPass && createdPass);
        }
    }
}
