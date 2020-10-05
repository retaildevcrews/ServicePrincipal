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
using CSE.Automation.Model;
using System.Collections.Generic;
using CSE.Automation.Processors;
using CSE.Automation.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

[assembly: FunctionsStartup(typeof(CSE.Automation.Startup))]

namespace CSE.Automation
{
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// Configure the host runtime
        /// </summary>
        /// <param name="builder"></param>
        /// <remarks>
        /// 1. Load configuration for host context
        /// 2. Register runtime services in container
        /// 3. Create runtime service settings instances and place in container
        /// 4. Enumerate all settings classes that are "validatable" and run their validator
        /// </remarks>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder == default)
                throw new ArgumentNullException(nameof(builder));

            Debug.WriteLine(Environment.GetEnvironmentVariable("AUTH_TYPE"));

            // CONFIGURATION
            BuildConfiguration(builder);

            // Settings from Config
            RegisterSettings(builder);

            RegisterServices(builder);
            //// Add key vault secrets to config object
            //var config = builder.AddAzureKeyVaultConfiguration("KeyVaultEndpoint");

            // Setup KV access and register services
            //ICredentialService credService = new CredentialService(credServiceSettings);
            //builder.Services.AddSingleton<ICredentialService>((s) => credService);

            //ISecretClient secretService = new SecretService(secretServiceSettings, credService);
            //builder.Services.AddSingleton<ISecretClient>((s) => secretService);




            // Retrieve CosmosDB configuration, create access objects, and register
            DALResolver dalResolver = new DALResolver(secretService);
            IDAL configDAL = dalResolver.GetService<IDAL> (DALCollection.Configuration.ToString());
            IDAL auditDAL = dalResolver.GetService <IDAL> (DALCollection.Audit.ToString());
            IDAL objTrackingDAL = dalResolver.GetService <IDAL> (DALCollection.ObjectTracking.ToString());

            builder.Services.AddSingleton<DALResolver>(dalResolver);

            // Create and register ProcessorResolver
            var processorResolver = new ProcessorResolver(configDAL);
            var processortest = processorResolver.GetService<IDeltaProcessor>(ProcessorType.ServicePrincipal.ToString());
            builder.Services.AddSingleton<ProcessorResolver>(processorResolver);


        }

        private void BuildConfiguration(IFunctionsHostBuilder builder)
        {
            // CONFIGURATION
            var serviceProvider = builder.Services.BuildServiceProvider();
            var env = serviceProvider.GetRequiredService<IHostingEnvironment>();
            var appDirectory = serviceProvider.GetRequiredService<IOptions<ExecutionContextOptions>>().Value.AppDirectory;
            var defaultConfig = serviceProvider.GetRequiredService<IConfiguration>();

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(appDirectory)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddJsonFile("local.settings.json", true)
                .AddConfiguration(defaultConfig)
                .AddAzureKeyVaultConfiguration("KeyVaultEndpoint");

            var hostConfig = configBuilder.Build();
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), hostConfig));
        }

        private void RegisterSettings(IFunctionsHostBuilder builder)
        {
            // SERVICES SETTINGS
            var secretServiceSettings = new SecretServiceSettings() { KeyVaultName = Environment.GetEnvironmentVariable(Constants.KeyVaultName) };
            var credServiceSettings = new CredentialServiceSettings() { AuthType = Environment.GetEnvironmentVariable(Constants.AuthType).As<AuthenticationType>()};
            builder.Services
                .AddSingleton(credServiceSettings)
                .AddSingleton(secretServiceSettings)
                .AddTransient<GraphHelperSettings>()
                .AddTransient(provider => new ConfigDBSettings(provider.GetService<ISecretClient>()){CollectionName = Constants.CosmosDBConfigCollectionName})
                .AddTransient(provider => new AuditDBSettings(provider.GetService<ISecretClient>()){CollectionName = Constants.CosmosDBConfigCollectionName})
                .AddTransient(provider => new TrackingDBSettings(provider.GetService<ISecretClient>()){CollectionName = Constants.CosmosDBConfigCollectionName});

        }

        private void RegisterServices(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddSingleton(typeof(ICredentialService), typeof(CredentialService))
                .AddSingleton(typeof(ISecretClient), typeof(SecretService))
                .AddSingleton<DALResolver>();

            builder.Services
                .AddTransient<IConfigDAL, DAL>(provider => new DAL(provider.GetService<ICosmosDBSettings>(), provider.GetService<ILogger<DAL>>()));
            IDAL configDAL = dalResolver.GetService<IDAL> (DALCollection.Configuration.ToString());
            IDAL auditDAL = dalResolver.GetService <IDAL> (DALCollection.Audit.ToString());
            IDAL objTrackingDAL = dalResolver.GetService <IDAL> (DALCollection.ObjectTracking.ToString());
            // Setup graph API helper and register
            builder.Services.AddSingleton(typeof(IGraphHelper<ServicePrincipal>), typeof(ServicePrincipalGraphHelper));
        }

    }
}
