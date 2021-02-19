// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CSE.Automation.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSE.Automation.Interfaces
{
    internal interface IObjectTrackingService
    {
#pragma warning disable SA1127 // Generic type constraints should be on their own line
        Task<TrackingModel> Get<TEntity>(string id) where TEntity : GraphModel;
        Task<TEntity> GetAndUnwrap<TEntity>(string id) where TEntity : GraphModel;
        Task<TrackingModel> Put<TEntity>(ActivityContext context, TEntity entity) where TEntity : GraphModel;
        Task<TrackingModel> Put(ActivityContext context, TrackingModel entity);
#pragma warning restore SA1127 // Generic type constraints should be on their own line
    }
}
