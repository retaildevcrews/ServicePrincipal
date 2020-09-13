using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using CSE.Automation.Interfaces;
using CSE.Automation.Services;
using CSE.Automation.Utilities;
using CSE.Automation.KeyVault;
using CSE.Automation.DataAccess;

[assembly: FunctionsStartup(typeof(CSE.Automation.Startup))]

namespace CSE.Automation
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            Debug.WriteLine(Environment.GetEnvironmentVariable("AUTH_TYPE"));

            //setup KV access and register services
            ICredentialService credService = new CredentialService(Environment.GetEnvironmentVariable(Constants.AuthType));
            builder.Services.AddSingleton<ICredentialService>((s) => credService);

            ISecretClient secretService = new SecretService(Environment.GetEnvironmentVariable(Constants.KeyVaultName), credService);
            builder.Services.AddSingleton<ISecretClient>((s) => secretService);

            //setup graph API helper and register
            var graphAppClientId = secretService.GetSecretValue(Constants.GraphAppClientIdKey);
            var graphAppTentantId = secretService.GetSecretValue(Constants.GraphAppTenantIdKey);
            var graphAppClientSecret = secretService.GetSecretValue(Constants.GraphAppClientSecretKey);

            var graphHelper =  new GraphHelper(SecureStringHelper.ConvertToUnsecureString(graphAppClientId), 
                                    SecureStringHelper.ConvertToUnsecureString(graphAppTentantId), 
                                    SecureStringHelper.ConvertToUnsecureString(graphAppClientSecret));

            builder.Services.AddSingleton<IGraphHelper>(graphHelper);

            //Retrieve CosmosDB configuration, create access objects, and register
            IDALResolver dalResolver = new DALResolver(secretService);
            IDAL configDAL = dalResolver.GetDAL(DALCollection.Configuration);
            IDAL auditDAL = dalResolver.GetDAL(DALCollection.Audit);
            IDAL objTrackingDAL = dalResolver.GetDAL(DALCollection.ObjectTracking);

            builder.Services.AddSingleton<IDALResolver>(dalResolver);
        }
    }
}