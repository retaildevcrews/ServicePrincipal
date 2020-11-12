using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.AuditResults
{
    internal class AuditResultValidator1 : AuditResultValidatorBase, IAuditResultValidator
    {
        public AuditResultValidator1(AuditEntry savedAuditEntry, AuditEntry newAuditEntry, ActivityContext activityContext, TestCase testCase) 
                : base(savedAuditEntry, newAuditEntry, activityContext, testCase)
        {
        }
        public override bool Validate()
        {
            //AUDIT PASS record exists with correlation id of activity

            bool typePass = (NewAuditEntry.Type == AuditActionType.Pass);

            bool isNewAuditEntry = NewAuditEntry.Timestamp > SavedAuditEntry.Timestamp;
            
            bool validCorrelationId = Guid.TryParse(NewAuditEntry.CorrelationId, out Guid dummyGuid) &&
                                        NewAuditEntry.CorrelationId.Equals(Context.CorrelationId);

            return (typePass && isNewAuditEntry && validCorrelationId);

        }
    }
}
