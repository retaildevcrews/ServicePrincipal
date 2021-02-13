using CSE.Automation.Model;
using CSE.Automation.Tests.UnitTests.TestCaseValidators.DataAccess;
using Microsoft.Graph;
using static CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.AuditResults
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
