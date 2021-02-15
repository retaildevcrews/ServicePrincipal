using System;
using CSE.Automation.Extensions;
using CSE.Automation.Model;
using CSE.Automation.Tests.IntegrationTests.TestCaseValidators.DataAccess;
using Microsoft.Graph;
using static CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.AuditResults.Discover
{
    internal class DiscoverAuditResultValidator2 : AuditResultValidatorBase, IAuditResultValidator
    {
        public DiscoverAuditResultValidator2(AuditEntry savedAuditEntry, AuditEntry newAuditEntry, ActivityContext activityContext,
                                        ServicePrincipal servicePrincipal, AuditRepositoryTest auditRepositoryTest, TestCase testCase)
                                        : base(savedAuditEntry, newAuditEntry, activityContext, servicePrincipal, auditRepositoryTest, testCase)
        {
        }
        public override bool Validate()
        {
            //  attributeName:"AdditionalData",
            //existingAttributeValue: "@removed"));


            bool typePass = (NewAuditEntry.Type == AuditActionType.Pass);


            bool validReasonPass = (NewAuditEntry.Reason == AuditCode.Deleted.Description());
                               

            bool isNewAuditEntryPass = NewAuditEntry.Timestamp > SavedAuditEntry.Timestamp;
            
            bool validCorrelationIdPass = Guid.TryParse(NewAuditEntry.Descriptor.CorrelationId, out Guid dummyGuid) &&
                                        NewAuditEntry.Descriptor.CorrelationId.Equals(Context.CorrelationId);

            return (typePass && isNewAuditEntryPass && validCorrelationIdPass && validReasonPass );

        }
    }
}
