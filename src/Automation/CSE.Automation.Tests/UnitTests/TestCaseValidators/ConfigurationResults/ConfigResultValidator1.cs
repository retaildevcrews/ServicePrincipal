using CSE.Automation.DataAccess;
using CSE.Automation.Model;
using static CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.ConfigurationResults
{

    internal class ConfigResultValidator1 : ConfigResultValidatorBase, IConfigResultValidator
    {
        public ConfigResultValidator1(ProcessorConfiguration savedConfigEntry, ProcessorConfiguration newConfigEntry, 
                                       ActivityContext activityContext, ConfigRepository configRepository, TestCase testCase)
                                        : base(savedConfigEntry, newConfigEntry, activityContext, configRepository, testCase)
        {
        }
        public override bool Validate()
        {

            //NOTE: SavedConfigEntry.LastSeedTime will be null if a new config item was created for this test case 
            bool isLastTimeSeedPass =  SavedConfigEntry.LastSeedTime != null ? NewConfigEntry.LastSeedTime > SavedConfigEntry.LastSeedTime :  true;

            bool dataLinkPass = string.IsNullOrEmpty(NewConfigEntry.DeltaLink) != true;

            bool runStatePass = NewConfigEntry.RunState == RunState.DeltaRun;

            return (isLastTimeSeedPass && dataLinkPass && runStatePass);
        }
    }
}
