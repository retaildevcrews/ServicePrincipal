// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CSE.Automation.Model;

namespace CSE.Automation.Interfaces
{
    internal interface IAuditRepository : ICosmosDBRepository<AuditEntry> { }
}
