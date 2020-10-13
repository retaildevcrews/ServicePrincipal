using System;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation.Extensions
{
    static class TypeExtensions
    {
        public static bool IsEnumeration(this Type type)
        {
            return !type.IsAbstract && type.IsEnum;
        }
    }
}
