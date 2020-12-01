using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.System.ComponentModel;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases
{
    internal class EvaluateTestCaseCollection : TestCaseCollection,  ITestCaseCollection, IDisposable
    {
        [SpStateDefinition("SpStateDefinition1")]
        [ObjectStateDefinition("ObjectStateDefinition1")]
        [SpValidator("SpResultValidator1")]
        [ObjectValidator("ObjectResultValidator1")]
        [AuditValidator("AuditResultValidator1")]
        public override TestCase TC1 => TestCase.TC1;

        [SpStateDefinition("SpStateDefinition2")]
        [ObjectStateDefinition("ObjectStateDefinition2")]
        [SpValidator("SpResultValidator2")]
        [ObjectValidator("ObjectResultValidator2")]
        [AuditValidator("AuditResultValidator2")]
        public override TestCase TC2 => TestCase.TC2;

        [SpStateDefinition("SpStateDefinition2_2")]
        [ObjectStateDefinition("ObjectStateDefinition2_2")]
        [SpValidator("SpResultValidator2_2")]
        [ObjectValidator("ObjectResultValidator2_2")]
        [AuditValidator("AuditResultValidator2_2")]
        public TestCase TC2_2 => TestCase.TC2_2; //<<<<<<<<<<<<<<<<<<<No override,  this Test Case is only applicable for Evaluate

        [SpStateDefinition("SpStateDefinition3")]
        [ObjectStateDefinition("ObjectStateDefinition3")]
        [SpValidator("SpResultValidator3")]
        [ObjectValidator("ObjectResultValidator3")]
        [AuditValidator("AuditResultValidator3")]
        public override TestCase TC3 => TestCase.TC3;

        [SpStateDefinition("SpStateDefinition3_2")]
        [ObjectStateDefinition("ObjectStateDefinition3_2")]
        [SpValidator("SpResultValidator3_2")]
        [ObjectValidator("ObjectResultValidator3_2")]
        [AuditValidator("AuditResultValidator3_2")]
        public TestCase TC3_2 => TestCase.TC3_2; //<<<<<<<<<<<<<<<<<<<No override,  this Test Case is only applicable for Evaluate

        [SpStateDefinition("SpStateDefinition4")]
        [ObjectStateDefinition("ObjectStateDefinition4")]
        [SpValidator("SpResultValidator4")]
        [ObjectValidator("ObjectResultValidator4")]
        [AuditValidator("AuditResultValidator4")]
        public override TestCase TC4 => TestCase.TC4;

        [SpStateDefinition("SpStateDefinition5")]
        [ObjectStateDefinition("ObjectStateDefinition5")]
        [SpValidator("SpResultValidator5")]
        [ObjectValidator("ObjectResultValidator5")]
        [AuditValidator("AuditResultValidator5")]
        public override TestCase TC5 => TestCase.TC5;

        [SpStateDefinition("SpStateDefinition6")]
        [ObjectStateDefinition("ObjectStateDefinition6")]
        [SpValidator("SpResultValidator6")]
        [ObjectValidator("ObjectResultValidator6")]
        [AuditValidator("AuditResultValidator6")]
        public override TestCase TC6 => TestCase.TC6;

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
