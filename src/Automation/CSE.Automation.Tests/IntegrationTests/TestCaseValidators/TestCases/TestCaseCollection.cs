namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases
{
    abstract internal class TestCaseCollection : ITestCaseCollection
    {
        public const string TestNewUserSuffix = "-TEST_NEW_OWNER";

        public const string TestRemovedAttributeSuffix = "-TEST_REMOVED_ATTRIBUTE";

        static internal string ServicePrincipalIdForTestNewUser { get; set; }

        //Full list of Test Cases 
        internal enum TestCase
        {
            TC1,
            TC1_2,
            TC2,
            TC2_2,
            TC3,
            TC3_2,
            TC4,
            TC5,
            TC6,
            TC7,
            TC8,
            TC9
        }

    
        public virtual TestCase TC1 => TestCase.TC1;
        public virtual TestCase TC2 => TestCase.TC2;
        public virtual TestCase TC3 => TestCase.TC3;
        public virtual TestCase TC4 => TestCase.TC4;
        public virtual TestCase TC5 => TestCase.TC5;
        public virtual TestCase TC6 => TestCase.TC6;
        public virtual TestCase TC7 => TestCase.TC7;
        public virtual TestCase TC8 => TestCase.TC8;
        public virtual TestCase TC9 => TestCase.TC9;
    }
}
