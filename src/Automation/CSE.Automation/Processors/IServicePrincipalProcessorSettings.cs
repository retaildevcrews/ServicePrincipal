// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace CSE.Automation.Processors
{
    internal interface IServicePrincipalProcessorSettings
    {
        string QueueConnectionString { get; set; }
        string EvaluateQueueName { get; set; }
        string UpdateQueueName { get; set; }
        string DiscoverQueueName { get; set; }
        UpdateMode AADUpdateMode { get; set; }
        Guid ConfigurationId { get; set; }
        int VisibilityDelayGapSeconds { get; set; }
        int QueueRecordProcessThreshold { get; set; }
        void Validate();
    }
}
