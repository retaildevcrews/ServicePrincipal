using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.System.ComponentModel;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases
{
    internal class DiscoverTestCaseCollection : TestCaseCollection, IDisposable
    {
        [SpStateDefinition("DiscoverSpStateDefinition1")]
        [SpValidator("DiscoverSpResultValidator1")]
        //[ConfigurationtValidator("")]
        //[ActivityValidator("")]
        //[AuditValidator("")]
        public override TestCase TC1 => TestCase.TC1;

        public TestCase TC1_2 => TestCase.TC1_2; //<<<<<<<<<<<<<<<<<<<No override,  this Test Case is only applicable for Discover

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
