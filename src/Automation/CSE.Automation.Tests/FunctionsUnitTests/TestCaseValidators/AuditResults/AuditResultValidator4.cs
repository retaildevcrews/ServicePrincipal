using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;
using CSE.Automation.Extensions;
using Microsoft.Graph;
using CSE.Automation.DataAccess;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.DataAccess;
using System.Linq;
using System.Threading.Tasks;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.AuditResults
{
    internal class AuditResultValidator4 : AuditResultValidatorBase, IAuditResultValidator
    {
        public AuditResultValidator4(AuditEntry savedAuditEntry, AuditEntry newAuditEntry, ActivityContext activityContext,
                                        ServicePrincipal servicePrincipal, AuditRepositoryTest auditRepositoryTest, TestCase testCase)
                                        : base(savedAuditEntry, newAuditEntry, activityContext, servicePrincipal, auditRepositoryTest, testCase)
        {
        }
        public override bool Validate()
        {
            int invalidEmailsCount = ServicePrincipalObject.Notes.Split(';').ToList().Count();

            Task<IEnumerable<AuditEntry>> getAuditItems = Task.Run(() => Repository.GetItemsAsync(ServicePrincipalObject.Id, Context.CorrelationId));
            getAuditItems.Wait();

            var auditNoteItems = getAuditItems.Result.Where(x => x.AttributeName == "Notes").ToList();
            if ( auditNoteItems.Count() != invalidEmailsCount)
            {
                return false;
            }

            foreach (var auditEntry in auditNoteItems)
            {
                bool typePass = (auditEntry.Type == AuditActionType.Fail);

                bool validReasonPass = (auditEntry.Reason == AuditCode.Fail_AttributeValidation.Description());

                bool isNewAuditEntryPass = auditEntry.Timestamp > SavedAuditEntry.Timestamp;


                if (!typePass || !validReasonPass ||  !isNewAuditEntryPass)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
