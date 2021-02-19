using CSE.Automation.DataAccess;
using CSE.Automation.Model;
using static CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ConfigurationResults
{
    abstract class ConfigResultValidatorBase : IConfigResultValidator
    {
        public TestCase TestCaseID { get; }
        public ProcessorConfiguration SavedConfigEntry { get; }
        public ProcessorConfiguration NewConfigEntry { get; }
        public ActivityContext Context { get; }

        public ConfigRepository Repository { get; }

        public ConfigResultValidatorBase(ProcessorConfiguration savedConfigEntry, ProcessorConfiguration newConfigEntry, ActivityContext activityContext,
                                        ConfigRepository configRepository, TestCase testCase)
        {
            SavedConfigEntry = savedConfigEntry;
            NewConfigEntry = newConfigEntry;
            TestCaseID = testCase;
            Context = activityContext;
            Repository = configRepository;
        }

        public abstract bool Validate();
    }
}
