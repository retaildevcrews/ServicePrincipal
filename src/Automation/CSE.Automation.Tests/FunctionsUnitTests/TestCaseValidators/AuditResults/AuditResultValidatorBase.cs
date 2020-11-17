using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.DataAccess;
using CSE.Automation.Model;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.AuditResults
{
    abstract class AuditResultValidatorBase : IAuditResultValidator
    {
        public TestCase TestCaseID { get; }
        public AuditEntry SavedAuditEntry { get; }
        public AuditEntry NewAuditEntry { get; }
        public ActivityContext Context { get; }

        public ServicePrincipal ServicePrincipalObject { get; }

        public AuditRepository Repository { get; }

        public AuditResultValidatorBase(AuditEntry savedAuditEntry, AuditEntry newAuditEntry, ActivityContext activityContext, 
                                        ServicePrincipal servicePrincipal, AuditRepository auditRepository, TestCase testCase)
        {
            SavedAuditEntry = savedAuditEntry;
            NewAuditEntry = newAuditEntry;
            TestCaseID = testCase;
            Context = activityContext;
            ServicePrincipalObject = servicePrincipal;
            Repository = auditRepository;
        }

        public abstract bool Validate();
    }
}
