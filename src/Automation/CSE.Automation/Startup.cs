using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using CSE.Automation.Interfaces;
using CSE.Automation.Services;
using CSE.Automation.Utilities;
using CSE.Automation.KeyVault;
using CSE.Automation.DataAccess;
using Microsoft.Graph;

[assembly: FunctionsStartup(typeof(CSE.Automation.Startup))]

namespace CSE.Automation
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder == default)
                throw new ArgumentNullException(nameof(builder));

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

            var graphHelper =  new ServicePrincipalGraphHelper(graphAppClientId, graphAppTentantId, graphAppClientSecret);

            builder.Services.AddSingleton<IGraphHelper<ServicePrincipal>>(graphHelper);

            //Retrieve CosmosDB configuration, create access objects, and register
            IServiceResolver dalResolver = new DALResolver(secretService);
            IDAL configDAL = dalResolver.GetService<IDAL> (DALCollection.Configuration.ToString());
            IDAL auditDAL = dalResolver.GetService <IDAL> (DALCollection.Audit.ToString());
            IDAL objTrackingDAL = dalResolver.GetService <IDAL> (DALCollection.ObjectTracking.ToString());

            builder.Services.AddSingleton<IServiceResolver>(dalResolver);
        }
    }
}