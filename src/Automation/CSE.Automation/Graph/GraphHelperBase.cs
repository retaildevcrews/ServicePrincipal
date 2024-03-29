﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using CSE.Automation.Extensions;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using SettingsBase = CSE.Automation.Model.SettingsBase;

namespace CSE.Automation.Graph
{
    internal abstract class GraphHelperBase<TEntity> : IGraphHelper<TEntity>
    {
        protected readonly ILogger logger;
        protected readonly IGraphHelperSettings settings;
        protected IAuditService auditService;

        protected IGraphServiceClient GraphClient { get; }

        protected GraphHelperBase(IGraphHelperSettings settings, IAuditService auditService, IGraphServiceClient graphClient, ILogger logger)
        {
            this.settings = settings;
            this.auditService = auditService;
            this.logger = logger;
            this.GraphClient = graphClient;
        }

        public abstract Task<(GraphOperationMetrics metrics, IEnumerable<TEntity> data)> GetDeltaGraphObjects(ActivityContext context, ProcessorConfiguration config);
        public abstract Task<(TEntity, IList<User>)> GetEntityWithOwners(string id);
        public abstract Task PatchGraphObject(TEntity entity);
    }
}
