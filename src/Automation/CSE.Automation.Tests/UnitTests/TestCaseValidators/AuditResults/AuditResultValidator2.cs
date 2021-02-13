using System;
using CSE.Automation.Extensions;
using CSE.Automation.Model;
using CSE.Automation.Tests.UnitTests.TestCaseValidators.DataAccess;
using Microsoft.Graph;
using static CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.AuditResults
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

            bool typePass = (NewAuditEntry.Type == AuditActionType.Pass);


            bool validReasonPass = (NewAuditEntry.Reason == AuditCode.Pass.Description());

            //SavedAuditEntry will be null when Audit Colection is empty
            bool isNewAuditEntryPass = SavedAuditEntry != null ? NewAuditEntry.Timestamp > SavedAuditEntry.Timestamp : true;
            
            bool validCorrelationIdPass = NewAuditEntry.Descriptor != null && Guid.TryParse(NewAuditEntry.Descriptor.CorrelationId, out Guid dummyGuid) &&
                                          NewAuditEntry.Descriptor.CorrelationId.Equals(Context.CorrelationId);

            return (typePass && isNewAuditEntryPass && validCorrelationIdPass && validReasonPass );

        }
    }
}
