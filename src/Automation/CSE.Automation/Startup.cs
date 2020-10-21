using System;
using System.Diagnostics;
using System.Linq;
using CSE.Automation.DataAccess;
using CSE.Automation.Extensions;
using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.KeyVault;
using CSE.Automation.Model;
using CSE.Automation.Processors;
using CSE.Automation.Services;
using CSE.Automation.Validators;

using FluentValidation;
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
            var envName = builder.GetContext().EnvironmentName;
            var appDirectory = builder.GetContext().ApplicationRootPath;
            var defaultConfig = serviceProvider.GetRequiredService<IConfiguration>();

            // order dependent.  Environment settings should override local configuration
            //  The reasoning for the order is a local config file is more difficult to change
            //  than an environment setting.  KeyVault settings should override any previous setting.
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(appDirectory)
                .AddConfiguration(defaultConfig)
                .AddAzureKeyVaultConfiguration(Constants.KeyVaultName)
                .AddJsonFile($"appsettings.{envName}.json", true);

            var hostConfig = configBuilder.Build();
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), hostConfig));
        }

        private static void RegisterSettings(IFunctionsHostBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var config = serviceProvider.GetRequiredService<IConfiguration>();

            // SERVICES SETTINGS
            var secretServiceSettings = new SecretServiceSettings() { KeyVaultName = config[Constants.KeyVaultName] };
            var credServiceSettings = new CredentialServiceSettings() { AuthType = config[Constants.AuthType].As<AuthenticationType>() };

            builder.Services
                .AddSingleton(credServiceSettings)
                .AddSingleton(secretServiceSettings)
                .AddSingleton<ISettingsValidator>(provider => provider.GetRequiredService<SecretServiceSettings>())

                .AddTransient<GraphHelperSettings>(x => new GraphHelperSettings(x.GetRequiredService<ISecretClient>())
                {
                    ScanLimit = config["ScanLimit"].ToInt()
                })
                .AddTransient<ISettingsValidator, GraphHelperSettings>()

                .AddSingleton<ConfigRespositorySettings>(x => new ConfigRespositorySettings(x.GetRequiredService<ISecretClient>())
                {
                    Uri = config[Constants.CosmosDBURLName],
                    Key = config[Constants.CosmosDBKeyName],
                    DatabaseName = config[Constants.CosmosDBDatabaseName],
                    CollectionName = config[Constants.CosmosDBConfigCollectionName]
                })
                .AddSingleton<ISettingsValidator>(provider => provider.GetRequiredService<ConfigRespositorySettings>())

                .AddSingleton<AuditRespositorySettings>(x => new AuditRespositorySettings(x.GetRequiredService<ISecretClient>()))
                .AddSingleton<ISettingsValidator>(provider => provider.GetRequiredService<AuditRespositorySettings>())

                .AddSingleton<ObjectTrackingRepositorySettings>(x => new ObjectTrackingRepositorySettings(x.GetRequiredService<ISecretClient>())
                {
                    Uri = config[Constants.CosmosDBURLName],
                    Key = config[Constants.CosmosDBKeyName],
                    DatabaseName = config[Constants.CosmosDBDatabaseName],
                    CollectionName = config[Constants.CosmosDBObjectTrackingCollectionName]
                })
                .AddSingleton<ISettingsValidator>(provider => provider.GetRequiredService<ObjectTrackingRepositorySettings>())

                .AddSingleton(x => new ServicePrincipalProcessorSettings(x.GetRequiredService<ISecretClient>())
                {
                    QueueConnectionString = config[Constants.SPStorageConnectionString],
                    QueueName = config[Constants.EvaluateQueueAppSetting.Trim('%')],
                    ConfigurationId = config["configId"].ToGuid(Guid.Parse("02a54ac9-441e-43f1-88ee-fde420db2559")),
                    VisibilityDelayGapSeconds = config["visibilityDelayGapSeconds"].ToInt(8),
                    QueueRecordProcessThreshold = config["queueRecordProcessThreshold"].ToInt(10),
                })
                .AddSingleton<ISettingsValidator>(provider => provider.GetRequiredService<ServicePrincipalProcessorSettings>());
            //.AddSingleton<ICosmosDBSettings, CosmosDBSettings>(x => new CosmosDBSettings(x.GetRequiredService<ISecretClient>())
            //                                                                {
            //                                                                    Uri = config[Constants.CosmosDBURLName],
            //                                                                    Key = config[Constants.CosmosDBKeyName],
            //                                                                    DatabaseName = config[Constants.CosmosDBDatabaseName],
            //                                                                })


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
                catch (Azure.Identity.CredentialUnavailableException credEx)
                {
                    _logger.LogCritical(credEx, $"Failed to validate application configuration: Azure Identity is incorrect.");
                    throw;
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
            // register the concrete as the singleton, then use forwarder pattern to register same singleton with alternate interfaces

            // Moved commented lines from line 173 to avoid lint error in CI
            // .AddSingleton<IConfigRepository>(provider => provider.GetService<ConfigRepository>())
            // .AddSingleton<ICosmosDBRepository>(provider => provider.GetService<ConfigRepository>())

            builder.Services
                .AddSingleton<ICredentialService>(x => new CredentialService(x.GetRequiredService<CredentialServiceSettings>()))
                .AddSingleton<ISecretClient>(x => new SecretService(x.GetRequiredService<SecretServiceSettings>(), x.GetRequiredService<ICredentialService>()))

                .AddSingleton<ConfigRepository>()
                .AddSingleton<IConfigRepository, ConfigRepository>(provider => provider.GetRequiredService<ConfigRepository>())
                .AddSingleton<ICosmosDBRepository<ProcessorConfiguration>, ConfigRepository>(provider => provider.GetRequiredService<ConfigRepository>())

                .AddSingleton<AuditRepository>()
                .AddSingleton<IAuditRepository, AuditRepository>(provider => provider.GetRequiredService<AuditRepository>())
                .AddSingleton<ICosmosDBRepository<AuditEntry>, AuditRepository>(provider => provider.GetRequiredService<AuditRepository>())

                .AddSingleton<ObjectTrackingRepository>()
                .AddSingleton<IObjectTrackingRepository, ObjectTrackingRepository>(provider => provider.GetRequiredService<ObjectTrackingRepository>())
                .AddSingleton<ICosmosDBRepository<TrackingModel>, ObjectTrackingRepository>(provider => provider.GetRequiredService<ObjectTrackingRepository>())

                .AddScoped<IGraphHelper<ServicePrincipal>, ServicePrincipalGraphHelper>()
                .AddScoped<IServicePrincipalProcessor, ServicePrincipalProcessor>()

                .AddScoped<IObjectTrackingService, ObjectTrackingService>()

                .AddScoped<IModelValidator<GraphModel>, GraphModelValidator>()
                .AddScoped<IModelValidator<ServicePrincipalModel>, ServicePrincipalModelValidator>()
                .AddScoped<IModelValidator<AuditEntry>, AuditEntryValidator>()
                .AddSingleton<IModelValidatorFactory, ModelValidatorFactory>()

                .AddScoped<GraphDeltaProcessor>()

                .AddTransient<IQueueServiceFactory, AzureQueueServiceFactory>();
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

        //    var repositories = provider.GetServices<IRepository>();
        //    var hasFailingTest = false;

        //    foreach (var repository in repositories)
        //    {
        //        var testPassed = repository.Test().Result;
        //        hasFailingTest = testPassed == false || hasFailingTest;

        //        var result = testPassed
        //            ? "Passed"
        //            : "Failed";
        //        var message = $"Repository test for {repository.Id} {result}";
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
