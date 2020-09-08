using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using CSE.Automation.Interfaces;
using CSE.Automation.Services;
using CSE.Automation.Utilities;
using CSE.Automation.KeyVault;

[assembly: FunctionsStartup(typeof(CSE.Automation.Startup))]

namespace CSE.Automation
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            Debug.WriteLine(Environment.GetEnvironmentVariable("AUTH_TYPE"));

            ICredentialService credService = new CredentialService(Environment.GetEnvironmentVariable("AUTH_TYPE"));
            builder.Services.AddSingleton<ICredentialService>((s) => credService);

            ISecretClient secretService = new SecretService(Environment.GetEnvironmentVariable("KEYVAULT_NAME"), credService);
            builder.Services.AddSingleton<ISecretClient>((s) => secretService);

            var graphAppClientId = secretService.GetSecretValue(Constants.GraphAppClientIdKey);
            var graphAppTentantId = secretService.GetSecretValue(Constants.GraphAppTenantIdKey);
            var graphAppClientSecret = secretService.GetSecretValue(Constants.GraphAppClientSecretKey);

            var graphHelper =  new GraphHelper(SecureStringHelper.ConvertToUnsecureString(graphAppClientId), 
                                    SecureStringHelper.ConvertToUnsecureString(graphAppTentantId), 
                                    SecureStringHelper.ConvertToUnsecureString(graphAppClientSecret));

            builder.Services.AddSingleton<IGraphHelper>(graphHelper);
        }
    }
}