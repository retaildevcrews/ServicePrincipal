// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using CSE.Automation.Graph;
using CSE.Automation.Model;

namespace CSE.Automation.Interfaces
{
    internal interface IGraphHelper<TEntity>
    {
        Task<(GraphOperationMetrics metrics, IEnumerable<TEntity> data)> GetDeltaGraphObjects(ActivityContext context, ProcessorConfiguration config);
        Task<TEntity> GetEntityWithOwners(string id);
        Task PatchGraphObject(TEntity entity);
    }
}
