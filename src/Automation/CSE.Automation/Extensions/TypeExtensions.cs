using System;

namespace CSE.Automation.Extensions
{
    internal static class TypeExtensions
    {
        public static bool IsEnumeration(this Type type)
        {
            return !type.IsAbstract && type.IsEnum;
        }
    }
}
