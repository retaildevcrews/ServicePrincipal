using System;
using CSE.Automation.Tests.UnitTests.TestCaseValidators.System.ComponentModel;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases
{
    internal class UpdateTestCaseCollection : TestCaseCollection,  ITestCaseCollection, IDisposable
    {
        [SpStateDefinition("UpdateSpStateDefinition1")]
        [SpValidator("UpdateSpResultValidator")]  // Common Result Validator class
        [AuditValidator("UpdateAuditResultValidator")]// Unique Audit Validator class
        [ObjectStateDefinition("ObjectStateDefinition3")]
        [ObjectValidator("ObjectResultValidator3")]
        public override TestCase TC1 => TestCase.TC1;


        [SpStateDefinition("UpdateSpStateDefinition2")]
        [SpValidator("UpdateSpResultValidator2")]  // Custom Validator UpdateSpResultValidator2
        [AuditValidator("UpdateAuditResultValidator")]// Unique Audit Validator class
        public override TestCase TC2 => TestCase.TC2;

        [SpStateDefinition("UpdateSpStateDefinition3")]
        [SpValidator("UpdateSpResultValidator")]  // Common Result Validator class 
        [AuditValidator("UpdateAuditResultValidator")]// Unique Audit Validator class
        public override TestCase TC3 => TestCase.TC3;

      

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
