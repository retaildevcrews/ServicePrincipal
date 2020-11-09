using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.AuditResults
{
    internal class AuditResultValidator1 : AuditResultValidatorBase, IAuditResultValidator
    {
        public AuditResultValidator1(AuditEntry savedAuditEntry, AuditEntry newAuditEntry, TestCase testCase) : base(savedAuditEntry, newAuditEntry, testCase)
        {
        }
        public override bool Validate()
        {
            //AUDIT PASS record exists with correlation id of activity
            bool typePass = (NewAuditEntry.Type == AuditActionType.Pass);

            bool isNewAuditEntry = NewAuditEntry.Timestamp > SavedAuditEntry.Timestamp;
            
            //Should AuditCorrelationId match something else? e.g ObjectTracking Id ?
            bool validCorrelationId = Guid.TryParse(NewAuditEntry.CorrelationId, out Guid dummyGuid);
            
            return (typePass && isNewAuditEntry && validCorrelationId);

        }
    }
}
