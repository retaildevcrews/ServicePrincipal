using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;
using CSE.Automation.Extensions;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.AuditResults
{
    internal class AuditResultValidator2 : AuditResultValidatorBase, IAuditResultValidator
    {
        public AuditResultValidator2(AuditEntry savedAuditEntry, AuditEntry newAuditEntry, ActivityContext activityContext, TestCase testCase) 
                : base(savedAuditEntry, newAuditEntry, activityContext, testCase)
        {
        }
        public override bool Validate()
        {

            bool typePass = (NewAuditEntry.Type == AuditActionType.Fail);


            bool validReason = (NewAuditEntry.Reason == AuditCode.Fail_AttributeValidation.Description() ||
                               NewAuditEntry.Reason == AuditCode.Fail_MissingOwners.Description());

            bool isNewAuditEntry = NewAuditEntry.Timestamp > SavedAuditEntry.Timestamp;
            
            bool validCorrelationId = Guid.TryParse(NewAuditEntry.CorrelationId, out Guid dummyGuid) &&
                                        NewAuditEntry.CorrelationId.Equals(Context.CorrelationId);

            return (typePass && isNewAuditEntry && validCorrelationId && validReason);

        }
    }
}
