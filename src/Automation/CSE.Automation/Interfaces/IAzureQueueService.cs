// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CSE.Automation.Model;
using System.Threading.Tasks;

namespace CSE.Automation.Interfaces
{
    public interface IAzureQueueService
    {
        public Task Send(QueueMessage message, int visibilityDelay = 0);
    }
}
