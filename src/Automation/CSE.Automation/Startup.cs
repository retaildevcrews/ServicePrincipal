using System;
using System.Diagnostics;

using CSE.Automation.DataAccess;
using CSE.Automation.Extensions;
using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.KeyVault;
using CSE.Automation.Model;
using CSE.Automation.Processors;
using CSE.Automation.Services;

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

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
            _logger.LogDebug($"AUTH_TYPE: {Environment.GetEnvironmentVariable("AUTH_TYPE")}");

            // CONFIGURATION
            BuildConfiguration(builder);

            // Settings from Config built above
            RegisterSettings(builder);

            RegisterServices(builder);

            ValidateSettings(builder);

            //ValidateServices(builder);

        }

        /// <summary>
        /// Create a basic logger (low dependency) so that we can get some logs out of bootstrap
        /// </summary>
        /// <returns></returns>
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
            var credServiceSettings = new CredentialServiceSettings() { AuthType = Environment.GetEnvironmentVariable(Constants.AuthType).As<AuthenticationType>() };

            builder.Services
                .AddSingleton(credServiceSettings)
                .AddSingleton(secretServiceSettings)
                .AddSingleton<ISettingsValidator>(provider => provider.GetRequiredService<SecretServiceSettings>())

                .AddTransient<GraphHelperSettings>()
                .AddTransient<ISettingsValidator, GraphHelperSettings>()

                .AddSingleton<ConfigRespositorySettings>(x => new ConfigRespositorySettings(x.GetRequiredService<ISecretClient>()))
                .AddSingleton<ISettingsValidator>(provider => provider.GetRequiredService<ConfigRespositorySettings>())

                .AddSingleton<AuditRespositorySettings>(x => new AuditRespositorySettings(x.GetRequiredService<ISecretClient>()))
                .AddSingleton<ISettingsValidator>(provider => provider.GetRequiredService<AuditRespositorySettings>())

                .AddSingleton(x => new ServicePrincipalProcessorSettings(x.GetRequiredService<ISecretClient>())
                {
                    ConfigurationId = Environment.GetEnvironmentVariable("configId").ToGuid(Guid.Parse("02a54ac9-441e-43f1-88ee-fde420db2559")),
                    VisibilityDelayGapSeconds = Environment.GetEnvironmentVariable("visibilityDelayGapSeconds").ToInt(8),
                    QueueRecordProcessThreshold = Environment.GetEnvironmentVariable("queueRecordProcessThreshold").ToInt(10),
                })
                .AddSingleton<ICosmosDBSettings, CosmosDBSettings>(x => new CosmosDBSettings(x.GetRequiredService<ISecretClient>()))
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
                .AddSingleton<ICredentialService>(x => new CredentialService(x.GetRequiredService<CredentialServiceSettings>()))
                .AddSingleton<ISecretClient>(x => new SecretService(x.GetRequiredService<SecretServiceSettings>(), x.GetRequiredService<ICredentialService>()))
                // register the concrete as the singleton, then use forwarder pattern to register same singleton with alternate interfaces
                .AddScoped<ConfigRepository>()
                .AddScoped<IConfigRepository, ConfigRepository>()
                .AddScoped<ICosmosDBRepository<ProcessorConfiguration>, ConfigRepository>()
                .AddScoped<AuditRepository>()
                .AddScoped<IAuditRepository, AuditRepository>()
                .AddScoped<ICosmosDBRepository<AuditEntry>, AuditRepository>()
                //.AddSingleton<IConfigRepository>(provider => provider.GetService<ConfigRepository>())
                //.AddSingleton<ICosmosDBRepository>(provider => provider.GetService<ConfigRepository>())
                .AddScoped<IGraphHelper<ServicePrincipal>, ServicePrincipalGraphHelper>()
                .AddScoped<IServicePrincipalProcessor, ServicePrincipalProcessor>()
                .AddScoped<GraphDeltaProcessor>();
        }

        /// <summary>
        /// Instantiate the remote data sources and verify their connectivity
        /// </summary>
        /// <param name="builder"></param>
        //private void ValidateServices(IFunctionsHostBuilder builder)
        //{
        //    foreach (var service in builder.Services)
        //    {
        //        Trace.WriteLine($"{service.ServiceType.Name}");
        //    }

        //    var provider = builder.Services.AddLogging(b =>
        //    {
        //        b.AddConsole();
        //        b.AddDebug();
        //    }).BuildServiceProvider();

        //    var repositories = provider.GetServices<ICosmosDBRepository<>>();
        //    var hasFailingTest = false;

        //    foreach (var repository in repositories)
        //    {
        //        var testPassed = repository.Test().Result;
        //        hasFailingTest = testPassed == false || hasFailingTest;

        //        var result = testPassed
        //            ? "Passed"
        //            : "Failed";
        //        var message = $"Repository test for {repository.DatabaseName}:{repository.CollectionName} {result}";
        //        if (testPassed)
        //        {
        //            _logger.LogInformation(message);
        //        }
        //        else
        //        {
        //            _logger.LogCritical(message);
        //        }
        //    }

        //    if (hasFailingTest)
        //        throw new ApplicationException($"One or more repositories failed test.");
        //}
    }
}
