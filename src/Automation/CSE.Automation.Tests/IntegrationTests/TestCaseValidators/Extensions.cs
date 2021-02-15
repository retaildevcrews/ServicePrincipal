using System;
using System.Collections.Generic;
using System.Linq;
using CSE.Automation.Model.Validators;
using CSE.Automation.Tests.IntegrationTests.TestCaseValidators.System.ComponentModel;
using CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators
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

            return attr?.SpStateDefinitionName;
        }

        public static string GetSpValidator(this ITestCaseCollection testCaseCollection, Enum value)
        {
            var type = testCaseCollection.GetType();

            var typeEnum = value.GetType();
            string nameEnum = Enum.GetName(typeEnum, value);

            var propInfo = type.GetProperty(nameEnum);

            var attr = Attribute.GetCustomAttribute(propInfo, typeof(SpValidatorAttribute)) as SpValidatorAttribute;

            return attr?.ValidatorName;
        }

        public static string GetObjectValidator(this ITestCaseCollection testCaseCollection, Enum value)
        {
            var type = testCaseCollection.GetType();

            var typeEnum = value.GetType();
            string nameEnum = Enum.GetName(typeEnum, value);

            var propInfo = type.GetProperty(nameEnum);

            var attr = Attribute.GetCustomAttribute(propInfo, typeof(ObjectValidatorAttribute)) as ObjectValidatorAttribute;

            return attr?.ValidatorName;
        }

        public static string GetAuditValidator(this ITestCaseCollection testCaseCollection, Enum value)
        {
            var type = testCaseCollection.GetType();

            var typeEnum = value.GetType();
            string nameEnum = Enum.GetName(typeEnum, value);

            var propInfo = type.GetProperty(nameEnum);

            var attr = Attribute.GetCustomAttribute(propInfo, typeof(AuditValidatorAttribute)) as AuditValidatorAttribute;

            return attr?.ValidatorName;
        }

        public static string GetObjectStateDefinition(this ITestCaseCollection testCaseCollection, Enum value)
        {
            var type = testCaseCollection.GetType();

            var typeEnum = value.GetType();
            string nameEnum = Enum.GetName(typeEnum, value);

            var propInfo = type.GetProperty(nameEnum);

            var attr = Attribute.GetCustomAttribute(propInfo, typeof(ObjectStateDefinitionAttribute)) as ObjectStateDefinitionAttribute;

            return attr?.ObjectStateDefinitionName;
        }

        public static string GetConfigValidator(this ITestCaseCollection testCaseCollection, Enum value)
        {
            var type = testCaseCollection.GetType();

            var typeEnum = value.GetType();
            string nameEnum = Enum.GetName(typeEnum, value);

            var propInfo = type.GetProperty(nameEnum);

            var attr = Attribute.GetCustomAttribute(propInfo, typeof(ConfigValidatorAttribute)) as ConfigValidatorAttribute;

            return attr?.ValidatorName;
        }

        public static string GetActivityValidator(this ITestCaseCollection testCaseCollection, Enum value)
        {
            var type = testCaseCollection.GetType();

            var typeEnum = value.GetType();
            string nameEnum = Enum.GetName(typeEnum, value);

            var propInfo = type.GetProperty(nameEnum);

            var attr = Attribute.GetCustomAttribute(propInfo, typeof(ActivityValidatorAttribute)) as ActivityValidatorAttribute;

            return attr?.ValidatorName;
        }

        public static List<string> GetAsList(this string notes)
        {
            return notes.Split(ServicePrincipalModelValidator.NotesSeparators).Select(x => x.Trim()).ToList();
        }
    }
}
