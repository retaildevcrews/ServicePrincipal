// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CSE.Automation.Graph;
using CSE.Automation.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSE.Automation.Interfaces
{
    internal interface IDeltaProcessor
    {
        int VisibilityDelayGapSeconds { get; }
        int QueueRecordProcessThreshold { get; }

        Task<IEnumerable<ActivityHistory>> GetActivityStatus(ActivityContext context, string activityId = null, string correlationId = null);
        Task RequestDiscovery(ActivityContext context, DiscoveryMode discoveryMode, string source);

        Task<GraphOperationMetrics> DiscoverDeltas(ActivityContext context, bool forceReseed = false);
        Task Lock(string lockingActivityID);
        Task Unlock();
    }
}
