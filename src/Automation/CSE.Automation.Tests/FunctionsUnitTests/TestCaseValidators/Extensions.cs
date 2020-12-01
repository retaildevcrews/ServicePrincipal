using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.System.ComponentModel;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators
{
    static class Extensions
    {

        public static string GetSpStateDefinition(this ITestCaseCollection testCaseCollection, Enum value)
        {
            var type = testCaseCollection.GetType();

            var typeEnum = value.GetType();
            string nameEnum = Enum.GetName(typeEnum, value);

            var propInfo = type.GetProperty(nameEnum);

            var attr = Attribute.GetCustomAttribute(propInfo, typeof(SpStateDefinitionAttribute)) as SpStateDefinitionAttribute;
            if (attr == null)
            {
                return null;
            }

            return attr.SpStateDefinitionName;
        }

        public static string GetSpValidator(this ITestCaseCollection testCaseCollection, Enum value)
        {
            var type = testCaseCollection.GetType();

            var typeEnum = value.GetType();
            string nameEnum = Enum.GetName(typeEnum, value);

            var propInfo = type.GetProperty(nameEnum);

            var attr = Attribute.GetCustomAttribute(propInfo, typeof(SpValidatorAttribute)) as SpValidatorAttribute;
            if (attr == null)
            {
                return null;
            }

            return attr.ValidatorName;
        }

        public static string GetObjectValidator(this ITestCaseCollection testCaseCollection, Enum value)
        {
            var type = testCaseCollection.GetType();

            var typeEnum = value.GetType();
            string nameEnum = Enum.GetName(typeEnum, value);

            var propInfo = type.GetProperty(nameEnum);

            var attr = Attribute.GetCustomAttribute(propInfo, typeof(ObjectValidatorAttribute)) as ObjectValidatorAttribute;
            if (attr == null)
            {
                return null;
            }

            return attr.ValidatorName;
        }

        public static string GetAuditValidator(this ITestCaseCollection testCaseCollection, Enum value)
        {
            var type = testCaseCollection.GetType();

            var typeEnum = value.GetType();
            string nameEnum = Enum.GetName(typeEnum, value);

            var propInfo = type.GetProperty(nameEnum);

            var attr = Attribute.GetCustomAttribute(propInfo, typeof(AuditValidatorAttribute)) as AuditValidatorAttribute;
            if (attr == null)
            {
                return null;
            }

            return attr.ValidatorName;
        }

        public static string GetObjectStateDefinition(this ITestCaseCollection testCaseCollection, Enum value)
        {
            var type = testCaseCollection.GetType();

            var typeEnum = value.GetType();
            string nameEnum = Enum.GetName(typeEnum, value);

            var propInfo = type.GetProperty(nameEnum);

            var attr = Attribute.GetCustomAttribute(propInfo, typeof(ObjectStateDefinitionAttribute)) as ObjectStateDefinitionAttribute;
            if (attr == null)
            {
                return null;
            }

            return attr.ObjectStateDefinitionName;
        }

    }
}
