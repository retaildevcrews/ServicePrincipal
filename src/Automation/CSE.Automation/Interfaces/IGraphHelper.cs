// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using CSE.Automation.Extensions;
using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using SettingsBase = CSE.Automation.Model.SettingsBase;

namespace CSE.Automation.Interfaces
{
    internal interface IGraphHelper<TEntity>
    {
        Task<(GraphOperationMetrics metrics, IEnumerable<TEntity> data)> GetDeltaGraphObjects(ActivityContext context, ProcessorConfiguration config, string selectFields = null);
        Task<TEntity> GetGraphObjectWithOwners(string id);
        Task PatchGraphObject(TEntity entity);
    }
}
