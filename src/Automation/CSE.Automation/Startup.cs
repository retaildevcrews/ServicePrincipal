using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using CSE.Automation.Interfaces;
using CSE.Automation.Services;
using CSE.Automation.Graph;
using CSE.Automation.KeyVault;
using CSE.Automation.DataAccess;
using Microsoft.Graph;
using CSE.Automation.Processors;
using CSE.Automation.Extensions;

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

            // Add key vault secrets to config object
            var config = builder.AddAzureKeyVaultConfiguration("KeyVaultEndpoint");

            // Setup KV access and register services
            ICredentialService credService = new CredentialService(Environment.GetEnvironmentVariable(Constants.AuthType));
            builder.Services.AddSingleton<ICredentialService>((s) => credService);

            ISecretClient secretService = new SecretService(Environment.GetEnvironmentVariable(Constants.KeyVaultName), credService);
            builder.Services.AddSingleton<ISecretClient>((s) => secretService);

            // Setup graph API helper and register
            var graphAppClientId = secretService.GetSecretValue(Constants.GraphAppClientIdKey);
            var graphAppTenantId = secretService.GetSecretValue(Constants.GraphAppTenantIdKey);
            var graphAppClientSecret = secretService.GetSecretValue(Constants.GraphAppClientSecretKey);

            var spGraphHelper = new ServicePrincipalGraphHelper(graphAppClientId, graphAppTenantId, graphAppClientSecret);

            builder.Services.AddSingleton<GraphHelperBase<ServicePrincipal>>(spGraphHelper);

            // Retrieve CosmosDB configuration, create access objects, and register
            DALResolver dalResolver = new DALResolver(secretService);
            IDAL configDAL = dalResolver.GetService<IDAL>(DALCollection.ProcessorConfiguration.ToString());
            IDAL auditDAL = dalResolver.GetService<IDAL>(DALCollection.Audit.ToString());
            IDAL objTrackingDAL = dalResolver.GetService<IDAL>(DALCollection.ObjectTracking.ToString());

            builder.Services.AddSingleton<DALResolver>(dalResolver);

            // Create and register ProcessorResolver
            var processorResolver = new ProcessorResolver(configDAL, secretService, spGraphHelper);
            builder.Services.AddSingleton<ProcessorResolver>(processorResolver);

        }
    }
}
