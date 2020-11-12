// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

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
