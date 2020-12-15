using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.System.ComponentModel;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases
{
    internal class UpdateTestCaseCollection : TestCaseCollection,  ITestCaseCollection, IDisposable
    {
        [SpStateDefinition("UpdateSpStateDefinition1")]
        [SpValidator("UpdateSpResultValidator")]  // Unique Result Validator class
        [AuditValidator("UpdateAuditResultValidator")]// Unique Audit Validator class
        public override TestCase TC1 => TestCase.TC1;


        [SpStateDefinition("UpdateSpStateDefinition2")]
        [SpValidator("UpdateSpResultValidator")]  // Unique Result Validator class
        [AuditValidator("UpdateAuditResultValidator")]// Unique Audit Validator class
        public override TestCase TC2 => TestCase.TC2;

        [SpStateDefinition("UpdateSpStateDefinition3")]
        [SpValidator("UpdateSpResultValidator")]  // Unique Result Validator class
        [AuditValidator("UpdateAuditResultValidator")]// Unique Audit Validator class
        public override TestCase TC3 => TestCase.TC3;

      

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
