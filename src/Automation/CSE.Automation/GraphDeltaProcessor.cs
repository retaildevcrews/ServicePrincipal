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
        private readonly ICredentialService _credService;
        private readonly ISecretClient _secretService;

        private readonly IGraphHelper _graphHelper;
        private readonly IDALResolver _DALResolver;

        public GraphDeltaProcessor(ISecretClient secretClient, ICredentialService credService, IGraphHelper graphHelper, IDALResolver dalResolver)
        {
            _credService = credService;
            _secretService = secretClient;
            _graphHelper = graphHelper;
            _DALResolver = dalResolver;
        }

        [FunctionName("ServicePrincipalDeltas")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Will add specific error in time.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Required as part of Trigger declaration.")]
        public void Run([TimerTrigger("0 */2 * * * *")] TimerInfo myTimer, ILogger log)
        {
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
        }
    }
}
