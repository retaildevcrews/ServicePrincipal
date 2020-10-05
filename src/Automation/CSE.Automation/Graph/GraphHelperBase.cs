using System.Collections.Generic;
using System.Threading.Tasks;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;

namespace CSE.Automation.Graph
{
    public class GraphHelperSettings : SettingsBase
    {
        public GraphHelperSettings(ISecretClient secretClient) : base(secretClient) { }

        [Secret(Constants.GraphAppClientIdKey)]
        public string GraphAppClientId => base.GetSecret();

        [Secret(Constants.GraphAppTenantIdKey)]
        public string GraphAppTenantId => base.GetSecret();

        [Secret(Constants.GraphAppClientSecretKey)]
        public string GraphAppClientSecret => base.GetSecret();
    }

    public interface IGraphHelper<T>
    {
        Task<(string, IEnumerable<T>)> GetDeltaGraphObjects(string selectFields, ProcessorConfiguration config);
    }

    public abstract class GraphHelperBase<T> : IGraphHelper<T>
    {
        protected GraphServiceClient graphClient { get; }

        protected GraphHelperBase(GraphHelperSettings settings)
        {
           IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
           .Create(settings.GraphAppClientId)
           .WithTenantId(settings.GraphAppTenantId)
           .WithClientSecret(settings.GraphAppClientSecret)
           .Build();

            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);
            graphClient = new GraphServiceClient(authProvider);
        }

        public abstract Task<(string,IEnumerable<T>)> GetDeltaGraphObjects(string selectFields,ProcessorConfiguration config);
    }
}
