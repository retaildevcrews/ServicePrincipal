using System;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators
{
    namespace System.ComponentModel
    {
        internal class StateDefinitionAttribute : Attribute
        {
            public string StateDefinitionName;
            public StateDefinitionAttribute(string stateDefinitionName)
            {
                StateDefinitionName = stateDefinitionName;
            }
        }

        internal class ValidatorAttribute : Attribute
        {
            public string ValidatorName;
            public ValidatorAttribute(string validatorName) 
            {
                ValidatorName = validatorName; 
            }
        }
    }
}
