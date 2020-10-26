﻿using System.Collections.Generic;
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
    public class GraphHelperSettings : SettingsBase
    {
        public GraphHelperSettings(ISecretClient secretClient) : base(secretClient) { }

        public int? ScanLimit { get; set; }

        [Secret(Constants.GraphAppClientIdKey)]
        public string GraphAppClientId => base.GetSecret();

        [Secret(Constants.GraphAppTenantIdKey)]
        public string GraphAppTenantId => base.GetSecret();

        [Secret(Constants.GraphAppClientSecretKey)]
        public string GraphAppClientSecret => base.GetSecret();

        public override void Validate()
        {
            if (GraphAppClientId.IsNull())
            {
                throw new ConfigurationErrorsException($"{GetType().Name}: GraphAppClientId is null");
            }

            if (GraphAppTenantId.IsNull())
            {
                throw new ConfigurationErrorsException($"{GetType().Name}: GraphAppTenantId is null");
            }

            if (GraphAppClientSecret.IsNull())
            {
                throw new ConfigurationErrorsException($"{GetType().Name}: GraphAppClientSecret is null");
            }
        }
    }

    public interface IGraphHelper<T>
    {
        Task<(string, IEnumerable<T>)> GetDeltaGraphObjects(ActivityContext context, ProcessorConfiguration config, string selectFields = null);
        Task<T> GetGraphObject(string id);
        Task PatchGraphObject(T entity);
    }

    internal abstract class GraphHelperBase<TEntity> : IGraphHelper<TEntity>
    {
        protected GraphServiceClient graphClient { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Used to super-classes")]
        protected readonly ILogger _logger;
        protected readonly GraphHelperSettings _settings;
        protected IAuditService _auditService;

        protected GraphHelperBase(GraphHelperSettings settings, IAuditService auditService, ILogger logger)
        {
            _settings = settings;
            _auditService = auditService;
            _logger = logger;
            var confidentialClientApplication = ConfidentialClientApplicationBuilder
#pragma warning disable CA1062 // Validate arguments of public methods, settings is injected from parent via Container
                                                                               .Create(settings.GraphAppClientId)
#pragma warning restore CA1062 // Validate arguments of public methods
                                                                               .WithTenantId(settings.GraphAppTenantId)
                                                                                .WithClientSecret(settings.GraphAppClientSecret)
                                                                                .Build();

            var authProvider = new ClientCredentialProvider(confidentialClientApplication);
            graphClient = new GraphServiceClient(authProvider);
        }

        public abstract Task<(string, IEnumerable<TEntity>)> GetDeltaGraphObjects(ActivityContext context, ProcessorConfiguration config, string selectFields = null);
        public abstract Task<TEntity> GetGraphObject(string id);
        public abstract Task PatchGraphObject(TEntity entity);

    }
}
