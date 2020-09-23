using System.Collections.Generic;
using System.Threading.Tasks;
using CSE.Automation.Model;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;

namespace CSE.Automation.Utilities
{
    public abstract class GraphHelperBase<T>
    {
        protected GraphServiceClient graphClient { get; }

        public GraphHelperBase(string graphAppClientId, string graphAppTenantId, string graphAppClientSecret)
        {
           IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
           .Create(graphAppClientId)
           .WithTenantId(graphAppTenantId)
           .WithClientSecret(graphAppClientSecret)
           .Build();

            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);
            graphClient = new GraphServiceClient(authProvider);
        }

        public abstract Task<(string,IEnumerable<T>)> GetDeltaGraphObjects(string selectFields,Configuration config);
    }
}