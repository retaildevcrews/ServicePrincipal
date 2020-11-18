// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Interfaces;
using Microsoft.Extensions.Logging;

namespace CSE.Automation.Services
{
    internal class AzureQueueServiceFactory : IQueueServiceFactory
    {
        private readonly ILogger<AzureQueueService> logger;
        public AzureQueueServiceFactory(ILogger<AzureQueueService> logger)
        {
            this.logger = logger;
        }

        public IAzureQueueService Create(string connectionString, string queueName)
        {
            return new AzureQueueService(connectionString, queueName, this.logger);
        }
    }
}
