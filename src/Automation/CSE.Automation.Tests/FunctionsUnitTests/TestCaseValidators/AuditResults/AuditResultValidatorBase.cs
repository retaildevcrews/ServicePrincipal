using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.DataAccess;
using CSE.Automation.Model;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.DataAccess;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.AuditResults
{
    abstract class AuditResultValidatorBase : IAuditResultValidator
    {
        public TestCase TestCaseID { get; }
        public AuditEntry SavedAuditEntry { get; }
        public AuditEntry NewAuditEntry { get; }
        public ActivityContext Context { get; }

        public ServicePrincipal ServicePrincipalObject { get; }

        public AuditRepositoryTest Repository { get; }

        public AuditResultValidatorBase(AuditEntry savedAuditEntry, AuditEntry newAuditEntry, ActivityContext activityContext, 
                                        ServicePrincipal servicePrincipal, AuditRepositoryTest auditRepositoryTest, TestCase testCase)
        {
            SavedAuditEntry = savedAuditEntry;
            NewAuditEntry = newAuditEntry;
            TestCaseID = testCase;
            Context = activityContext;
            ServicePrincipalObject = servicePrincipal;
            Repository = auditRepositoryTest;
        }

        public abstract bool Validate();
    }
}
