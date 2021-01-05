// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Graph;

namespace CSE.Automation.Model
{
    internal static class ServicePrincipalComparer
    {
        /// <summary>
        /// Use default comparision logic when determining ServicePrincipal equality.
        ///
        /// First, GetHashCode will be called for an initial equality check.  Since the default implementation
        /// uses the hashcode of the Id, it is determining equality if the Ids of the objects match (case insensitive)
        /// </summary>
        public class DefaultComparer : IEqualityComparer<ServicePrincipal>
        {
            public virtual bool Equals(ServicePrincipal x, ServicePrincipal y)
            {
                if (x == y)
                {
                    return true;
                }

                return x != null && y != null;
            }

            public int GetHashCode(ServicePrincipal obj)
            {
                if (obj == null)
                {
                    throw new ArgumentNullException(nameof(obj));
                }

                return string.GetHashCode(obj.Id, StringComparison.OrdinalIgnoreCase);
            }
        }

        public static IEqualityComparer<ServicePrincipal> Default { get; } = new DefaultComparer();
    }
}
