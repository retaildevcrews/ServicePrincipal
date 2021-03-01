// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using CSE.Automation.DataAccess;
using CSE.Automation.Extensions;
using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.KeyVault;
using CSE.Automation.Model;
using CSE.Automation.Model.Validators;
using CSE.Automation.Processors;
using CSE.Automation.Services;
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
        private ILogger logger;

        /// <summary>
        /// Configure the host runtime
        /// </summary>
        /// <param name="builder">An instance of <see cref="IFunctionsHostBuilder"/>.</param>
        /// <remarks>
        /// 1. Load configuration for host context
        /// 2. Register runtime services in container
        /// 3. Create runtime service settings instances and place in container
        /// 4. Enumerate all settings classes that are "validatable" and run their validator
        /// </remarks>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder == default)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            logger = CreateBootstrapLogger();
            logger.LogInformation($"Bootstrap logger initialized.");
            logger.LogDebug($"AUTH_TYPE: {Environment.GetEnvironmentVariable("AUTH_TYPE")}");

            // CONFIGURATION
            BuildConfiguration(builder);

            // Settings from Config built above
            RegisterSettings(builder);

            RegisterServices(builder);

            ValidateSettings(builder);
        }

        /// <summary>
        /// Create a basic logger (low dependency) so that we can get some logs out of bootstrap
        /// </summary>
        /// <returns>An instance of <see cref="ILogger"/>.</returns>
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
                .AddJsonFile($"appsettings.{envName}.json", true)
                .AddConfiguration(defaultConfig)
                .AddAzureKeyVaultConfiguration(Constants.KeyVaultName);

            // file only exists on local dev machine, so treat these as dev machine overrides
            //  the environment must be set to Development for this file to be even considered!
            if (string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase))
            {
                configBuilder
                    .AddJsonFile($"appsettings.Development.json", true);
            }

            var hostConfig = configBuilder.Build();
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), hostConfig));
        }

        private static void RegisterSettings(IFunctionsHostBuilder builder)
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            var versionMetadata = new VersionMetadata(thisAssembly);
            var logger = CreateBootstrapLogger();
            logger.LogInformation(JsonSerializer.Serialize(versionMetadata.ProductVersion));
            var serviceProvider = builder.Services.BuildServiceProvider();
            var config = serviceProvider.GetRequiredService<IConfiguration>();

            // SERVICES SETTINGS
            var secretServiceSettings = new SecretServiceSettings() { KeyVaultName = config[Constants.KeyVaultName] };
            var credServiceSettings = new CredentialServiceSettings() { AuthType = config[Constants.AuthType].As<AuthenticationType>() };

            builder.Services
                .AddSingleton<VersionMetadata>(versionMetadata)
                .AddSingleton(credServiceSettings)
                .AddSingleton(secretServiceSettings)
                .AddTransient<ISettingsValidator>(provider => provider.GetRequiredService<SecretServiceSettings>())

                .AddSingleton<GraphHelperSettings>(x => new GraphHelperSettings(x.GetRequiredService<ISecretClient>())
                {
                    VerboseLogging = bool.TryParse(config[Constants.GraphAppVerboseLogging], out bool value) && value,
                })
                .AddSingleton<IGraphHelperSettings>(provider => provider.GetRequiredService<GraphHelperSettings>())
                .AddTransient<ISettingsValidator, GraphHelperSettings>()

                .AddSingleton<ConfigRespositorySettings>(x => new ConfigRespositorySettings(x.GetRequiredService<ISecretClient>())
                {
                    Uri = config[Constants.CosmosDBURLName],
                    Key = config[Constants.CosmosDBKeyName],
                    DatabaseName = config[Constants.CosmosDBDatabaseName],
                    CollectionName = config[Constants.CosmosDBConfigCollectionName],
                })
                .AddTransient<ISettingsValidator>(provider => provider.GetRequiredService<ConfigRespositorySettings>())

                .AddSingleton<AuditRepositorySettings>(x => new AuditRepositorySettings(x.GetRequiredService<ISecretClient>())
                {
                    Uri = config[Constants.CosmosDBURLName],
                    Key = config[Constants.CosmosDBKeyName],
                    DatabaseName = config[Constants.CosmosDBDatabaseName],
                    CollectionName = config[Constants.CosmosDBAuditCollectionName],
                })
                .AddTransient<ISettingsValidator>(provider => provider.GetRequiredService<AuditRepositorySettings>())

                .AddSingleton<ObjectTrackingRepositorySettings>(x => new ObjectTrackingRepositorySettings(x.GetRequiredService<ISecretClient>())
                {
                    Uri = config[Constants.CosmosDBURLName],
                    Key = config[Constants.CosmosDBKeyName],
                    DatabaseName = config[Constants.CosmosDBDatabaseName],
                    CollectionName = config[Constants.CosmosDBObjectTrackingCollectionName],
                })
                .AddTransient<ISettingsValidator>(provider => provider.GetRequiredService<ObjectTrackingRepositorySettings>())

                .AddSingleton<ActivityHistoryRepositorySettings>(x => new ActivityHistoryRepositorySettings(x.GetRequiredService<ISecretClient>())
                {
                    Uri = config[Constants.CosmosDBURLName],
                    Key = config[Constants.CosmosDBKeyName],
                    DatabaseName = config[Constants.CosmosDBDatabaseName],
                    CollectionName = config[Constants.CosmosDBActivityHistoryCollectionName],
                })
                .AddTransient<ISettingsValidator>(provider => provider.GetRequiredService<ActivityHistoryRepositorySettings>())

                .AddSingleton(x => new ServicePrincipalProcessorSettings(x.GetRequiredService<ISecretClient>())
                {
                    QueueConnectionString = config[Constants.SPStorageConnectionString],
                    EvaluateQueueName = config[Constants.EvaluateQueueAppSetting.Trim('%')],
                    UpdateQueueName = config[Constants.UpdateQueueAppSetting.Trim('%')],
                    DiscoverQueueName = config[Constants.DiscoverQueueAppSetting.Trim('%')],
                    ConfigurationId = config["configId"].ToGuid(Guid.Parse("02a54ac9-441e-43f1-88ee-fde420db2559")),
                    VisibilityDelayGapSeconds = config["visibilityDelayGapSeconds"].ToInt(8),
                    QueueRecordProcessThreshold = config["queueRecordProcessThreshold"].ToInt(10),
                    AADUpdateMode = config["aadUpdateMode"].As<UpdateMode>(UpdateMode.Update),
                })
                .AddSingleton<IServicePrincipalProcessorSettings>(provider => provider.GetRequiredService<ServicePrincipalProcessorSettings>())
                .AddTransient<ISettingsValidator>(provider => provider.GetRequiredService<ServicePrincipalProcessorSettings>());
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
                    logger.LogCritical(credEx, $"Failed to validate application configuration: Azure Identity is incorrect.");
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, $"Failed to validate application configuration");
                    throw;
                }
            }

            logger.LogInformation($"All settings classes validated.");
        }

        private static void RegisterServices(IFunctionsHostBuilder builder)
        {
            string path = builder.GetContext().ApplicationRootPath;

            // register the concrete as the singleton, then use forwarder pattern to register same singleton with alternate interfaces
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

                .AddSingleton<ActivityHistoryRepository>()
                .AddSingleton<IActivityHistoryRepository, ActivityHistoryRepository>(provider => provider.GetRequiredService<ActivityHistoryRepository>())
                .AddSingleton<ICosmosDBRepository<ActivityHistory>, ActivityHistoryRepository>(provider => provider.GetRequiredService<ActivityHistoryRepository>())

                .AddScoped<ConfigService>()
                .AddScoped<IConfigService<ProcessorConfiguration>, ConfigService>()

                .AddSingleton<IGraphServiceClient, GraphClient>()
                .AddScoped<IServicePrincipalGraphHelper, ServicePrincipalGraphHelper>()
                .AddScoped<IGraphHelper<User>, UserGraphHelper>()
                .AddScoped<IServicePrincipalProcessor, ServicePrincipalProcessor>()

                .AddScoped<IObjectTrackingService, ObjectTrackingService>()
                .AddScoped<IAuditService, AuditService>()
                .AddScoped<IActivityService, ActivityService>()

                .AddScoped<IModelValidator<GraphModel>, GraphModelValidator>()
                .AddScoped<IModelValidator<ServicePrincipalModel>, ServicePrincipalModelValidator>()
                .AddScoped<IModelValidator<AuditEntry>, AuditEntryValidator>()
                .AddSingleton<IModelValidatorFactory, ModelValidatorFactory>()

                .AddScoped<GraphDeltaProcessor>()

                .AddScoped<IServicePrincipalClassifier, ServicePrincipalClassifier>()

                .AddTransient<IQueueServiceFactory, AzureQueueServiceFactory>();
        }
    }
}
