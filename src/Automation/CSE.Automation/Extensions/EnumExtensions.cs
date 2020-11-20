// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;

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
