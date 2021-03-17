// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSE.Automation.Model;
using Microsoft.Azure.KeyVault;
using Microsoft.OData.UriParser;

namespace CSE.Automation.Interfaces
{
    internal interface IActivityService
    {
        Task<ActivityHistory> Put(ActivityHistory document);
        Task<ActivityHistory> Get(string id);
        Task<IEnumerable<ActivityHistory>> GetCorrelated(string correlationId);
        ActivityContext CreateContext(string name, string source, string correlationId = null, bool withTracking = false);
    }
}
