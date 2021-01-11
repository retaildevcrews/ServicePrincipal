using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.DataAccess;
using CSE.Automation.Extensions;
using CSE.Automation.Model;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.DataAccess;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.AuditResults
{
    internal class UpdateAuditResultValidator : AuditResultValidatorBase, IAuditResultValidator
    {
        public UpdateAuditResultValidator(AuditEntry savedAuditEntry, AuditEntry newAuditEntry, ActivityContext activityContext,
                                        ServicePrincipal servicePrincipal, AuditRepositoryTest auditRepositoryTest, TestCase testCase) 
                                        : base(savedAuditEntry, newAuditEntry, activityContext, servicePrincipal, auditRepositoryTest, testCase)
        {
        }
        public override bool Validate()
        {


            bool typePass = (NewAuditEntry.Type == AuditActionType.Change);

            bool attributeNamePass = NewAuditEntry.AttributeName == "Notes";


            bool validReasonPass = (NewAuditEntry.Reason == AuditCode.Updated.Description());

            bool isNewAuditEntryPass = NewAuditEntry.Timestamp > SavedAuditEntry.Timestamp;
            
            bool validCorrelationIdPass = NewAuditEntry.Descriptor != null && Guid.TryParse(NewAuditEntry.Descriptor.CorrelationId, out Guid dummyGuid) &&
                                        NewAuditEntry.Descriptor.CorrelationId.Equals(Context.CorrelationId);

            return (attributeNamePass && typePass && isNewAuditEntryPass && validCorrelationIdPass && validReasonPass);

        }
    }
}
