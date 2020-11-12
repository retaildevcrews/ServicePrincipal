﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSE.Automation.Graph
{
    internal class UserGraphHelper : GraphHelperBase<User>
    {
        public UserGraphHelper(GraphHelperSettings settings, IAuditService auditService, ILogger<UserGraphHelper> logger)
                : base(settings, auditService, logger)
        {
        }

        public override Task<(GraphOperationMetrics metrics, IEnumerable<User> data)> GetDeltaGraphObjects(ActivityContext context, ProcessorConfiguration config, string selectFields = null)
        {
            throw new NotImplementedException();
        }

        public async override Task<User> GetGraphObjectWithOwners(string id)
        {
            try
            {
                var value = await GraphClient.Users[id]
                    .Request()
                    .GetAsync()
                    .ConfigureAwait(false);
                return value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public override Task PatchGraphObject(User entity)
        {
            throw new NotImplementedException();
        }
    }
}
