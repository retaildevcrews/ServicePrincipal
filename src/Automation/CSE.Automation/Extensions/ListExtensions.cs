// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CSE.Automation.Extensions
{
    internal static class ListExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty<T>(this IList<T> list)
        {
            return list == null || list.Count == 0;
        }
    }
}
