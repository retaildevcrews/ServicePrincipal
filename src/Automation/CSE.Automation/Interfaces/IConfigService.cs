﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSE.Automation.Interfaces
{
    internal interface IConfigService<TConfig>
        where TConfig : class
    {
        Task<TConfig> Put(TConfig newDocument);
        TConfig Get(string id, ProcessorType processorType, string defaultConfigResourceName, bool createIfNotFound = false);
        Task Lock(string configId, string lockingActivityID, string defaultConfigResourceName);
        Task Unlock(string configId);
    }
}
