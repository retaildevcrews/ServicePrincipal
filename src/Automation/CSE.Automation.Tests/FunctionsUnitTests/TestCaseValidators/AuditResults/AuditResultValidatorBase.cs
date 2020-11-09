using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.AuditResults
{
    abstract class AuditResultValidatorBase : IAuditResultValidator
    {
        public TestCase TestCaseID { get; }

        public AuditEntry SavedAuditEntry { get; }

        public AuditEntry NewAuditEntry { get; }

        public AuditResultValidatorBase(AuditEntry savedAuditEntry, AuditEntry newAuditEntry, TestCase testCase)
        {
            SavedAuditEntry = savedAuditEntry;
            NewAuditEntry = newAuditEntry;
            TestCaseID = testCase;
        }

        public abstract bool Validate();
    }
}
