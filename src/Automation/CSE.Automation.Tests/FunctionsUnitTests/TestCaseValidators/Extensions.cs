using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.System.ComponentModel;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators
{
    static class Extensions
    {
     
        public static string GetStateDefinition(this Enum value)
        {
            var type = value.GetType();

            string name = Enum.GetName(type, value);
            if (name == null)
            {
                return null;
            }

            var field = type.GetField(name);
            if (field == null)
            {
                return null;
            }

            var attr = Attribute.GetCustomAttribute(field, typeof(StateDefinitionAttribute)) as StateDefinitionAttribute;
            if (attr == null)
            {
                return null;
            }

            return attr.StateDefinitionName;
        }

        public static string GetValidator(this Enum value)
        {
            var type = value.GetType();
            string name = Enum.GetName(type, value);

            if (name == null)
            {
                return null;
            }

            var field = type.GetField(name);
            if (field == null)
            {
                return null;
            }

            var attr = Attribute.GetCustomAttribute(field, typeof(ValidatorAttribute)) as ValidatorAttribute;
            if (attr == null)
            {
                return null;
            }

            return attr.ValidatorName;
        }
    }
}
