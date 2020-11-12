// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace CSE.Automation.Model
{
    internal class ServicePrincipalClassification
    {
        public Guid OwningTenant { get; set; }
        public string Type { get; set; }
        public ObjectClassification Classification { get; set; }
        public string Category { get; set; }
        public string OwningTenantDomain { get; set; }
        public bool HasOwner { get; set; }
    }
}
