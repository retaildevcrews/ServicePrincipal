﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSE.Automation.Extensions;
using CSE.Automation.Model;
using CSE.Automation.Tests.IntegrationTests.TestCaseValidators.DataAccess;
using Microsoft.Graph;
using static CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.AuditResults
{
    internal class AuditResultValidator3_2 : AuditResultValidatorBase, IAuditResultValidator
    {
        public AuditResultValidator3_2(AuditEntry savedAuditEntry, AuditEntry newAuditEntry, ActivityContext activityContext,
                                        ServicePrincipal servicePrincipal, AuditRepositoryTest auditRepositoryTest, TestCase testCase)
                                        : base(savedAuditEntry, newAuditEntry, activityContext, servicePrincipal, auditRepositoryTest, testCase)
        {
        }
        public override bool Validate()
        {
            int invalidEmailsCount = ServicePrincipalObject.Notes.GetAsList().Count();

            Task<IEnumerable<AuditEntry>> getAuditItems = Task.Run(() => Repository.GetItemsAsync(ServicePrincipalObject.Id, Context.CorrelationId));
            getAuditItems.Wait();

            if (getAuditItems.Result.Count() != invalidEmailsCount)
            {
                return false;
            }


            foreach (var auditEntry in getAuditItems.Result)
            {
                bool typePass = (auditEntry.Type == AuditActionType.Fail);

                bool validReasonPass = (auditEntry.Reason == AuditCode.AttributeValidation.Description());

                bool validAttributeNamePass = (auditEntry.AttributeName == "Notes");

                bool isNewAuditEntryPass = SavedAuditEntry != null ? auditEntry.Timestamp > SavedAuditEntry.Timestamp : true;

                if (!typePass || !validReasonPass || !validAttributeNamePass || !isNewAuditEntryPass)
                {
                    return false;
                }
            }

            return true;

        }
    }
}
