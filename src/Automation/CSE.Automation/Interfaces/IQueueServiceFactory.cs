// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation.Interfaces
{
    public interface IQueueServiceFactory
    {
        IAzureQueueService Create(string connectionString, string queueName);
    }
}
