﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CSE.Automation.Extensions
{
    internal static class EnumExtensions
    {
        public static string Description(this Enum value)
        {
            // get the field 
            var field = value.GetType().GetField(value.ToString());
            var customAttributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (customAttributes.Length > 0)
            {
                return (customAttributes[0] as DescriptionAttribute).Description;
            }
            else
            {
                return value.ToString();
            }
        }
    }
}