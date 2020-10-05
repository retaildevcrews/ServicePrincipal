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
        private ILogger _logger;

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

            _logger = CreateBootstrapLogger();
            _logger.LogInformation($"Bootstrap logger initialized.");
            Debug.WriteLine(Environment.GetEnvironmentVariable("AUTH_TYPE"));

            // CONFIGURATION
            BuildConfiguration(builder);

            // Settings from Config built above
            RegisterSettings(builder);

            RegisterServices(builder);

            ValidateSettings(builder);

            //// Add key vault secrets to config object
            //var config = builder.AddAzureKeyVaultConfiguration("KeyVaultEndpoint");

            // Setup KV access and register services
            //ICredentialService credService = new CredentialService(credServiceSettings);
            //builder.Services.AddSingleton<ICredentialService>((s) => credService);

            //ISecretClient secretService = new SecretService(secretServiceSettings, credService);
            //builder.Services.AddSingleton<ISecretClient>((s) => secretService);




            //// Retrieve CosmosDB configuration, create access objects, and register
            //DALResolver dalResolver = new DALResolver(secretService);
            //IDAL configDAL = dalResolver.GetService<IDAL> (DALCollection.Configuration.ToString());
            //IDAL auditDAL = dalResolver.GetService <IDAL> (DALCollection.Audit.ToString());
            //IDAL objTrackingDAL = dalResolver.GetService <IDAL> (DALCollection.ObjectTracking.ToString());

            //builder.Services.AddSingleton<DALResolver>(dalResolver);

            //// Create and register ProcessorResolver
            //var processorResolver = new ProcessorResolver(configDAL);
            //var processortest = processorResolver.GetService<IDeltaProcessor>(ProcessorType.ServicePrincipal.ToString());
            //builder.Services.AddSingleton<ProcessorResolver>(processorResolver);


        }

        private static ILogger CreateBootstrapLogger()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                })
                .BuildServiceProvider();

            return serviceProvider.GetService<ILoggerFactory>().CreateLogger<Startup>();
        }
        private static void BuildConfiguration(IFunctionsHostBuilder builder)
        {
            // CONFIGURATION
            var serviceProvider = builder.Services.BuildServiceProvider();
            var env = builder.GetContext().EnvironmentName;
            var appDirectory = builder.GetContext().ApplicationRootPath;
            var defaultConfig = serviceProvider.GetRequiredService<IConfiguration>();

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(appDirectory)
                .AddJsonFile($"appsettings.{env}.json", true)
                .AddJsonFile("local.settings.json", true)
                .AddConfiguration(defaultConfig)
                .AddAzureKeyVaultConfiguration("KeyVaultEndpoint");

            var hostConfig = configBuilder.Build();
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), hostConfig));
        }

        private static void RegisterSettings(IFunctionsHostBuilder builder)
        {
            // SERVICES SETTINGS
            var secretServiceSettings = new SecretServiceSettings() { KeyVaultName = Environment.GetEnvironmentVariable(Constants.KeyVaultName) };
            var credServiceSettings = new CredentialServiceSettings() { AuthType = Environment.GetEnvironmentVariable(Constants.AuthType).As<AuthenticationType>()};
            builder.Services
                .AddSingleton(credServiceSettings)
                .AddSingleton(secretServiceSettings)
                .AddSingleton<ISettingsValidator>(provider => provider.GetRequiredService<SecretServiceSettings>())

                .AddTransient<GraphHelperSettings>()
                .AddTransient<ISettingsValidator, GraphHelperSettings>()

                .AddSingleton<ICosmosDBSettings, CosmosDBSettings>()
                .AddSingleton<ISettingsValidator>(provider => provider.GetRequiredService<ICosmosDBSettings>());

        }

        private void ValidateSettings(IFunctionsHostBuilder builder)
        {
            var provider = builder.Services.BuildServiceProvider();
            var settings = provider.GetServices<ISettingsValidator>();
            foreach (var validator in settings)
            {
                try
                {
                    validator.Validate();
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, $"Failed to validate application configuration");
                    throw;
                }
            }

            _logger.LogInformation($"All settings classes validated.");
        }

        private static void RegisterServices(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddSingleton(typeof(ICredentialService), typeof(CredentialService))
                .AddSingleton(typeof(ISecretClient), typeof(SecretService))
                .AddSingleton<IConfigRepository, ConfigRepository>()

                .AddTransient<IGraphHelper<ServicePrincipal>, ServicePrincipalGraphHelper>()
                .AddTransient<IServicePrincipalProcessor, ServicePrincipalProcessor>();
                
        }

    }
}
