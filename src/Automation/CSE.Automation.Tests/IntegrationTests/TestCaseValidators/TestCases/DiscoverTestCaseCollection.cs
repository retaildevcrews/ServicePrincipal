using System;
using CSE.Automation.Tests.IntegrationTests.TestCaseValidators.System.ComponentModel;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases
{
    internal class DiscoverTestCaseCollection : TestCaseCollection, IDisposable
    {
        [SpStateDefinition("DiscoverSpStateDefinition1")]
        [SpValidator("DiscoverSpResultValidator1")]
        [ConfigValidator("ConfigResultValidator1")]
        [ActivityValidator("ActivityResultValidator1")]
        public override TestCase TC1 => TestCase.TC1;

        [SpStateDefinition("DiscoverSpStateDefinition1_2")]
        [SpValidator("DiscoverSpResultValidator1_2")]
        [ConfigValidator("ConfigResultValidator1_2")]
        [ActivityValidator("ActivityResultValidator1_2")]
        public TestCase TC1_2 => TestCase.TC1_2; //<<<<<<<<<<<<<<<<<<<No override,  this Test Case is only applicable for Discover

        [SpStateDefinition("DiscoverSpStateDefinition2")]
        [SpValidator("DiscoverSpResultValidator2")]
        [ConfigValidator("ConfigResultValidator2")]
        [ActivityValidator("ActivityResultValidator2")]
        [AuditValidator("DiscoverAuditResultValidator2")]
        public override TestCase TC2 => TestCase.TC2;

        [SpStateDefinition("DiscoverSpStateDefinition3")]
        [SpValidator("DiscoverSpResultValidator3")]
        [ConfigValidator("ConfigResultValidator3")]
        [ActivityValidator("ActivityResultValidator3")]
        [AuditValidator("DiscoverAuditResultValidator3")]
        public override TestCase TC3 => TestCase.TC3;

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
