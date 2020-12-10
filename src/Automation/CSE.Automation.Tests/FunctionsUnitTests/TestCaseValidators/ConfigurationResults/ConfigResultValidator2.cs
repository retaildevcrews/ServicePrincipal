using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.DataAccess;
using CSE.Automation.Model;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ConfigurationResults
{

    internal class ConfigResultValidator2 : ConfigResultValidatorBase, IConfigResultValidator
    {
        public ConfigResultValidator2(ProcessorConfiguration savedConfigEntry, ProcessorConfiguration newConfigEntry, 
                                       ActivityContext activityContext, ConfigRepository configRepository, TestCase testCase)
                                        : base(savedConfigEntry, newConfigEntry, activityContext, configRepository, testCase)
        {
        }
        public override bool Validate()
        {

            bool isLastTimeSeedPass = NewConfigEntry.LastSeedTime > SavedConfigEntry.LastSeedTime;

            bool dataLinkPass = string.IsNullOrEmpty(NewConfigEntry.DeltaLink) != true;

            bool runStatePass = NewConfigEntry.RunState == RunState.DeltaRun;

            return (isLastTimeSeedPass && dataLinkPass && runStatePass);

        }
    }
}
