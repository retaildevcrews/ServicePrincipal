﻿using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ObjectTrackingResults
{
    internal class ObjectResultValidator2_2 : ObjectResultValidatorBase, IObjectResultValidator
    {

        public ObjectResultValidator2_2(TrackingModel savedObjectTracking, TrackingModel newObjectTracking, ActivityContext activityContext, TestCase testCase)
            : base(savedObjectTracking, newObjectTracking, activityContext, testCase)
        {
        }

        public override bool Validate()
        {

            bool objectExistsPass = NewObjectTracking != null;


            bool validCorrelationIdPass = Guid.TryParse(NewObjectTracking.CorrelationId, out Guid dummyGuid) &&
                                        NewObjectTracking.CorrelationId.Equals(Context.CorrelationId);

            bool updatedPass = NewObjectTracking.LastUpdated > SavedObjectTracking.LastUpdated &&
                            NewObjectTracking.LastUpdated == NewObjectTracking.Created;//Different timestamp

            return (objectExistsPass && validCorrelationIdPass && updatedPass);
        }
    }
}
