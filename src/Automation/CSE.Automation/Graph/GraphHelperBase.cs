// Copyright (c) Microsoft Corporation. All rights reserved.
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
        protected GraphServiceClient GraphClient { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Used to super-classes")]
        protected readonly ILogger logger;
        protected readonly GraphHelperSettings settings;
        protected IAuditService auditService;

        protected GraphHelperBase(GraphHelperSettings settings, IAuditService auditService, ILogger logger)
        {
            this.settings = settings;
            this.auditService = auditService;
            this.logger = logger;
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
#pragma warning disable CA1062 // Validate arguments of public methods, settings is injected from parent via Container
                                                                               .Create(settings.GraphAppClientId)
#pragma warning restore CA1062 // Validate arguments of public methods
                                                                               .WithTenantId(settings.GraphAppTenantId)
                                                                                .WithClientSecret(settings.GraphAppClientSecret)
                                                                                .Build();

            // TODO: move these to Container
            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);
            GraphClient = new GraphServiceClient(authProvider);
        }

        public abstract Task<(GraphOperationMetrics metrics, IEnumerable<TEntity> data)> GetDeltaGraphObjects(ActivityContext context, ProcessorConfiguration config, string displayNamePatternFilter = null, string selectFields = null);
        public abstract Task<TEntity> GetGraphObjectWithOwners(string id);
        public abstract Task PatchGraphObject(TEntity entity);
    }
}
