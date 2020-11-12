// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSE.Automation.Model;

namespace CSE.Automation.Interfaces
{
    internal interface IActivityHistoryRepository : ICosmosDBRepository<ActivityHistory>
    {
        Task<IEnumerable<ActivityHistory>> GetCorrelated(string correlationId);
    }
}
