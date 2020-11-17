using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;
using CSE.Automation.Extensions;
using Microsoft.Graph;
using CSE.Automation.DataAccess;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.DataAccess;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.AuditResults
{
    internal class AuditResultValidator2 : AuditResultValidatorBase, IAuditResultValidator
    {
        public AuditResultValidator2(AuditEntry savedAuditEntry, AuditEntry newAuditEntry, ActivityContext activityContext,
                                        ServicePrincipal servicePrincipal, AuditRepositoryTest auditRepositoryTest, TestCase testCase)
                                        : base(savedAuditEntry, newAuditEntry, activityContext, servicePrincipal, auditRepositoryTest, testCase)
        {
        }
        public override bool Validate()
        {

            bool typePass = (NewAuditEntry.Type == AuditActionType.Fail);


            bool validReasonPass = (NewAuditEntry.Reason == AuditCode.Fail_AttributeValidation.Description() ||
                               NewAuditEntry.Reason == AuditCode.Fail_MissingOwners.Description());

            bool validAttributeNamePass = (NewAuditEntry.AttributeName == "Owners");

            bool isNewAuditEntryPass = NewAuditEntry.Timestamp > SavedAuditEntry.Timestamp;
            
            bool validCorrelationIdPass = Guid.TryParse(NewAuditEntry.CorrelationId, out Guid dummyGuid) &&
                                        NewAuditEntry.CorrelationId.Equals(Context.CorrelationId);

            return (typePass && isNewAuditEntryPass && validCorrelationIdPass && validReasonPass && validAttributeNamePass);

        }
    }
}
