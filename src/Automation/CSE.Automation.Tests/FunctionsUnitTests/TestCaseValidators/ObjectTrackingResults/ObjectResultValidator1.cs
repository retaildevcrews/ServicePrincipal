using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ObjectTrackingResults
{
    internal class ObjectResultValidator1 : ObjectResultValidatorBase, IObjectResultValidator
    {
        public ObjectResultValidator1(TrackingModel savedObjectTracking, TrackingModel newObjectTracking, TestCase testCase) : base(savedObjectTracking, newObjectTracking, testCase)
        {
        }

        public override bool Validate()
        {
            /*
             *Record exists for SP-1
                LastUpdated is within (tolerance) seconds
              CorrelationId = correlation id of activity 

             */
            return true;
        }
    }
}
