using System.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;

namespace CSE.Automation
{
    public static class GraphDeltaProcessor
    {
        private static string clientId = ConfigurationManager.AppSettings.Get("clientId");
        private static string tenantId = ConfigurationManager.AppSettings.Get("tenantId");
        private static string clientSecret = ConfigurationManager.AppSettings.Get("clientSecret");

        [FunctionName("ServicePrincipalDeltas")]
        public static void Run([TimerTrigger("0 */2 * * * *")] TimerInfo myTimer, ILogger log)
        {
            if(string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientSecret))
            {
                log.LogError("Error: Credentials Missing.");
                    return;
            }

            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
           .Create(clientId)
           .WithTenantId(tenantId)
           .WithClientSecret(clientSecret)
           .Build();

            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);

            // Initialize Graph client
            //GraphHelper.Initialize(authProvider);


            


        }
    }
}
