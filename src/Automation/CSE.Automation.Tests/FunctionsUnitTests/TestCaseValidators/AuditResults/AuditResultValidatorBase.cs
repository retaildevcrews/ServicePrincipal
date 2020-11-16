using System;
using System.Collections.Generic;
using System.Text;
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

        public AuditResultValidatorBase(AuditEntry savedAuditEntry, AuditEntry newAuditEntry, ActivityContext activityContext, TestCase testCase)
        {
            SavedAuditEntry = savedAuditEntry;
            NewAuditEntry = newAuditEntry;
            TestCaseID = testCase;
            Context = activityContext;
        }

        public abstract bool Validate();
    }
}
