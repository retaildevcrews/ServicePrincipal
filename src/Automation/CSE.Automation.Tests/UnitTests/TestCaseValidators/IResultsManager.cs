namespace CSE.Automation.Tests.UnitTests.TestCaseValidators
{
    internal interface IResultsManager
    {
        void SaveState();

        bool Validate();
    }
}
