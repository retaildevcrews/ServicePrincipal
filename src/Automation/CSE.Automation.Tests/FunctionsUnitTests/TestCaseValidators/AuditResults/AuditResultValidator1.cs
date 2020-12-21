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
    internal class AuditResultValidator1 : AuditResultValidatorBase, IAuditResultValidator
    {
        public AuditResultValidator1(AuditEntry savedAuditEntry, AuditEntry newAuditEntry, ActivityContext activityContext,
                                        ServicePrincipal servicePrincipal, AuditRepositoryTest auditRepositoryTest, TestCase testCase) 
                                        : base(savedAuditEntry, newAuditEntry, activityContext, servicePrincipal, auditRepositoryTest, testCase)
        {
        }
        public override bool Validate()
        {


            bool typePass = (NewAuditEntry.Type == AuditActionType.Pass);
            
            bool validReasonPass = (NewAuditEntry.Reason == AuditCode.Pass_ServicePrincipal.Description());

            //SavedAuditEntry will be null when Audit Colection is empty
            bool isNewAuditEntryPass = SavedAuditEntry != null ? NewAuditEntry.Timestamp > SavedAuditEntry.Timestamp : true;
            
            bool validCorrelationIdPass = Guid.TryParse(NewAuditEntry.CorrelationId, out Guid dummyGuid) &&
                                        NewAuditEntry.CorrelationId.Equals(Context.CorrelationId);

            return (typePass && isNewAuditEntryPass && validCorrelationIdPass && validReasonPass);

        }
    }
}
