namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators
{
    internal interface IResultsManager
    {
        void SaveState();

        bool Validate();
    }
}
