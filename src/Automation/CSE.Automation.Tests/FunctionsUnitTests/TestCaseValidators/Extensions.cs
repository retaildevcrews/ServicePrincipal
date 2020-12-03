using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.System.ComponentModel;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators
{
    static class Extensions
    {
     
        public static string GetSpStateDefinition(this Enum value)
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

            var attr = Attribute.GetCustomAttribute(field, typeof(SpStateDefinitionAttribute)) as SpStateDefinitionAttribute;
            if (attr == null)
            {
                return null;
            }

            return attr.SpStateDefinitionName;
        }

        public static string GetSpValidator(this Enum value)
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

            var attr = Attribute.GetCustomAttribute(field, typeof(SpValidatorAttribute)) as SpValidatorAttribute;
            if (attr == null)
            {
                return null;
            }

            return attr.ValidatorName;
        }

        public static string GetObjectValidator(this Enum value)
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

            var attr = Attribute.GetCustomAttribute(field, typeof(ObjectValidatorAttribute)) as ObjectValidatorAttribute;
            if (attr == null)
            {
                return null;
            }

            return attr.ValidatorName;
        }

        public static string GetAuditValidator(this Enum value)
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

            var attr = Attribute.GetCustomAttribute(field, typeof(AuditValidatorAttribute)) as AuditValidatorAttribute;
            if (attr == null)
            {
                return null;
            }

            return attr.ValidatorName;
        }

        public static string GetObjectStateDefinition(this Enum value)
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

            var attr = Attribute.GetCustomAttribute(field, typeof(ObjectStateDefinitionAttribute)) as ObjectStateDefinitionAttribute;
            if (attr == null)
            {
                return null;
            }

            return attr.ObjectStateDefinitionName;
        }
    }
}
