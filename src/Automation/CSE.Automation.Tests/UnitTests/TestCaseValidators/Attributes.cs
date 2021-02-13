using System;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators
{
    namespace System.ComponentModel
    {
        internal class SpStateDefinitionAttribute : Attribute
        {
            public string SpStateDefinitionName;
            public SpStateDefinitionAttribute(string spStateDefinitionName)
            {
                SpStateDefinitionName = spStateDefinitionName;
            }
        }

        internal class SpValidatorAttribute : Attribute
        {
            public string ValidatorName;
            public SpValidatorAttribute(string validatorName) 
            {
                ValidatorName = validatorName; 
            }
        }

        internal class ObjectValidatorAttribute : Attribute
        {
            public string ValidatorName;
            public ObjectValidatorAttribute(string validatorName)
            {
                ValidatorName = validatorName;
            }
        }

        internal class AuditValidatorAttribute : Attribute
        {
            public string ValidatorName;
            public AuditValidatorAttribute(string validatorName)
            {
                ValidatorName = validatorName;
            }
        }

        internal class ObjectStateDefinitionAttribute : Attribute
        {
            public string ObjectStateDefinitionName;
            public ObjectStateDefinitionAttribute(string objectStateDefinitionName)
            {
                ObjectStateDefinitionName = objectStateDefinitionName;
            }
        }

        internal class ConfigValidatorAttribute : Attribute
        {
            public string ValidatorName;
            public ConfigValidatorAttribute(string validatorName)
            {
                ValidatorName = validatorName;
            }
        }

        internal class ActivityValidatorAttribute : Attribute
        {
            public string ValidatorName;
            public ActivityValidatorAttribute(string validatorName)
            {
                ValidatorName = validatorName;
            }
        }
    }
}
