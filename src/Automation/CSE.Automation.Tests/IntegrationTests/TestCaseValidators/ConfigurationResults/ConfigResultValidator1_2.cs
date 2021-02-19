using CSE.Automation.DataAccess;
using CSE.Automation.Model;
using static CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ConfigurationResults
{

    internal class ConfigResultValidator1_2 : ConfigResultValidatorBase, IConfigResultValidator
    {
        public ConfigResultValidator1_2(ProcessorConfiguration savedConfigEntry, ProcessorConfiguration newConfigEntry, 
                                       ActivityContext activityContext, ConfigRepository configRepository, TestCase testCase)
                                        : base(savedConfigEntry, newConfigEntry, activityContext, configRepository, testCase)
        {
        }
        public override bool Validate()
        {
            
            //NOTE: SavedConfigEntry.LastSeedTime will be null if a new config item was created for this test case 
            bool isLastDeltaRunPass =  SavedConfigEntry.LastDeltaRun != null ? NewConfigEntry.LastDeltaRun > SavedConfigEntry.LastDeltaRun :  true;

            bool dataLinkPass = string.IsNullOrEmpty(NewConfigEntry.DeltaLink) != true;

            bool runStatePass = NewConfigEntry.RunState == RunState.DeltaRun;

            return (isLastDeltaRunPass && dataLinkPass && runStatePass);

        }
    }
}
