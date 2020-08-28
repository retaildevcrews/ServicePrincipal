using System;
using System.Configuration;
using System.Diagnostics;
using System.Security;
using CSE.Automation.Interfaces;
using CSE.Automation.Utilities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;

namespace CSE.Automation
{
    public class GraphDeltaProcessor
    {
        private static string clientId = ConfigurationManager.AppSettings.Get("clientId");
        private static string tenantId = ConfigurationManager.AppSettings.Get("tenantId");
        private static string clientSecret = ConfigurationManager.AppSettings.Get("clientSecret");

        private readonly ICredentialService _credService = default;
        private readonly ISecretClient _secretService = default;

        public GraphDeltaProcessor(ISecretClient secretClient, ICredentialService credService)
        {
            _credService = credService;
            _secretService = secretClient;
        }

        [FunctionName("ServicePrincipalDeltas")]
        public void Run([TimerTrigger("0 */2 * * * *")] TimerInfo myTimer, ILogger log)
        {
            //if(string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientSecret))
            //{
            //    log.LogError("Error: Credentials Missing.");
            //        return;
            //}
            try
            {
                var kvSecret = _secretService.GetSecret("testSecret");
                SecureString secureValue = _secretService.GetSecretValue("testSecret");
                Debug.WriteLine(SecureStringHelper.ConvertToUnsecureString(secureValue));
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                Debug.WriteLine(ex.Message);
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
