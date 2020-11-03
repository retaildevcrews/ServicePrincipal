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
    public class GraphHelperSettings : SettingsBase
    {
        public GraphHelperSettings(ISecretClient secretClient)
                : base(secretClient)
        {
        }

        [Secret(Constants.GraphAppClientIdKey)]
        public string GraphAppClientId => GetSecret();

        [Secret(Constants.GraphAppTenantIdKey)]
        public string GraphAppTenantId => GetSecret();

        [Secret(Constants.GraphAppClientSecretKey)]
        public string GraphAppClientSecret => GetSecret();

        public override void Validate()
        {
            if (this.GraphAppClientId.IsNull())
                throw new ConfigurationErrorsException($"{this.GetType().Name}: GraphAppClientId is null");
            if (this.GraphAppTenantId.IsNull())
                throw new ConfigurationErrorsException($"{this.GetType().Name}: GraphAppTenantId is null");
            if (this.GraphAppClientSecret.IsNull())
                throw new ConfigurationErrorsException($"{this.GetType().Name}: GraphAppClientSecret is null");
        }
    }

    internal interface IGraphHelper<TEntity>
    {
        Task<(GraphOperationMetrics metrics, IEnumerable<TEntity> data)> GetDeltaGraphObjects(ActivityContext context, ProcessorConfiguration config, string selectFields = null);
        Task<TEntity> GetGraphObject(string id);
        Task PatchGraphObject(TEntity entity);
    }

    internal abstract class GraphHelperBase<TEntity> : IGraphHelper<TEntity>
    {

        protected GraphServiceClient GraphClient { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Used to super-classes")]
        protected readonly ILogger _logger;
        protected readonly GraphHelperSettings _settings;
        protected IAuditService _auditService;

        protected GraphHelperBase(GraphHelperSettings settings, IAuditService auditService, ILogger logger)
        {
            _settings = settings;
            _auditService = auditService;
            _logger = logger;
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

        public abstract Task<(GraphOperationMetrics metrics, IEnumerable<TEntity> data)> GetDeltaGraphObjects(ActivityContext context, ProcessorConfiguration config, string selectFields = null);
        public abstract Task<TEntity> GetGraphObject(string id);
        public abstract Task PatchGraphObject(TEntity entity);
    }
}
