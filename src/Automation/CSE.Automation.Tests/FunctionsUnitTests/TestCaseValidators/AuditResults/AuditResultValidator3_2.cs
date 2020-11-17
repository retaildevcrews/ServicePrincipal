﻿using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;
using CSE.Automation.Extensions;
using Microsoft.Graph;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.AuditResults
{
    internal class AuditResultValidator3_2 : AuditResultValidatorBase, IAuditResultValidator
    {
        public AuditResultValidator3_2(AuditEntry savedAuditEntry, AuditEntry newAuditEntry, ActivityContext activityContext, TestCase testCase)
                                        : base(savedAuditEntry, newAuditEntry, activityContext, testCase)
        {
        }
        public override bool Validate()
        {
            // Do we want to go to the next validation step?? 
            // there will be same number of Audit Entries as Number of errors generated by invalid emails 
            // we can get the number of invalid emails from ServicePrincipal and comapare against number of Audit items that match Context.CorrelationId


            bool typePass = (NewAuditEntry.Type == AuditActionType.Fail);


            bool validReason = (NewAuditEntry.Reason == AuditCode.Fail_AttributeValidation.Description());

            bool validAttributeName = (NewAuditEntry.AttributeName == "Notes");

            bool isNewAuditEntry = NewAuditEntry.Timestamp > SavedAuditEntry.Timestamp;
            
            bool validCorrelationId = Guid.TryParse(NewAuditEntry.CorrelationId, out Guid dummyGuid) &&
                                        NewAuditEntry.CorrelationId.Equals(Context.CorrelationId);

            return (typePass && isNewAuditEntry && validCorrelationId && validReason && validAttributeName);

        }
    }
}
