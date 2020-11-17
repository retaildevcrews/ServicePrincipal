using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;
using CSE.Automation.Extensions;
using Microsoft.Graph;
using CSE.Automation.DataAccess;
using System.Threading.Tasks;
using System.Linq;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.AuditResults
{
    internal class AuditResultValidator3_2 : AuditResultValidatorBase, IAuditResultValidator
    {
        public AuditResultValidator3_2(AuditEntry savedAuditEntry, AuditEntry newAuditEntry, ActivityContext activityContext,
                                        ServicePrincipal servicePrincipal, AuditRepository auditRepository, TestCase testCase)
                                        : base(savedAuditEntry, newAuditEntry, activityContext, servicePrincipal, auditRepository, testCase)
        {
        }
        public override bool Validate()
        {

            int invalidEmailsCount = ServicePrincipalObject.Notes.Split(';').ToList().Count();

            Task<int> getAuditCount = Task.Run(() => Repository.GetCountAsync(ServicePrincipalObject.Id, Context.CorrelationId));
            getAuditCount.Wait();

            int auditEntriesCreated =  getAuditCount.Result;

            bool auditCountPass = auditEntriesCreated == invalidEmailsCount;

            bool typePass = (NewAuditEntry.Type == AuditActionType.Fail);

            bool validReasonPass = (NewAuditEntry.Reason == AuditCode.Fail_AttributeValidation.Description());

            bool validAttributeNamePass = (NewAuditEntry.AttributeName == "Notes");

            bool isNewAuditEntryPass = NewAuditEntry.Timestamp > SavedAuditEntry.Timestamp;

            bool validCorrelationIdPass = Guid.TryParse(NewAuditEntry.CorrelationId, out Guid dummyGuid) &&
                                        NewAuditEntry.CorrelationId.Equals(Context.CorrelationId);

            return (typePass && isNewAuditEntryPass && validCorrelationIdPass && validReasonPass && validAttributeNamePass && auditCountPass);

        }
    }
}
