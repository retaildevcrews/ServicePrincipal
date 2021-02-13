using System;
using CSE.Automation.DataAccess;
using CSE.Automation.Extensions;
using CSE.Automation.Interfaces;
using CSE.Automation.KeyVault;
using CSE.Automation.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CSE.Automation.Tests.Fixtures
{
    public class UnitTestFixture : IDisposable
    {
        internal IHost Host { get; private set; }
        internal IConfigurationRoot Config { get; private set; }

        internal IHost BuildHost(Func<IConfigurationRoot> configFunc = null, Action<IConfigurationRoot, IServiceCollection> serviceFunc = null)
        {
            var builder = CreateHostBuilder(configFunc, serviceFunc);
            Host = builder.Build();
            return Host;
        }

        private IHostBuilder CreateHostBuilder(Func<IConfigurationRoot> configFunc = null, Action<IConfigurationRoot, IServiceCollection> serviceFunc=null)
        {
            Config = configFunc?.Invoke();

            //// SERVICES SETTINGS
            //var secretServiceSettings = new SecretServiceSettings() { KeyVaultName = _config[Constants.KeyVaultName] };
            //var credServiceSettings = new CredentialServiceSettings() { AuthType = AuthenticationType.CLI };

            //var credService = new CredentialService(credServiceSettings);
            //var secretService = new SecretService(secretServiceSettings,credService) ;

            return
                new HostBuilder()
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                    builder.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    serviceFunc?.Invoke(Config, services);
                    //services
                    //    //.AddSingleton(credServiceSettings)
                    //    //.AddSingleton(secretServiceSettings)
                    //    //.AddSingleton(credService)
                    //    //.AddSingleton(secretService)
                    //    //.AddSingleton<ICredentialService>(credService)
                    //    //.AddSingleton<ISecretClient>(secretService)
                    //    .AddSingleton<ConfigRepository>()
                    //    .AddSingleton<IConfigRepository, ConfigRepository>(provider => provider.GetRequiredService<ConfigRepository>())
                    //    .AddSingleton<ConfigRespositorySettings>(x => new ConfigRespositorySettings(x.GetRequiredService<ISecretClient>())
                    //    {
                    //        Uri = _config[Constants.CosmosDBURLName],
                    //        Key = _config[Constants.CosmosDBKeyName],
                    //        DatabaseName = _config[Constants.CosmosDBDatabaseName],
                    //        CollectionName = _config[Constants.CosmosDBConfigCollectionName]
                    //    })
                    //    .AddScoped<ConfigService>();
                });

        }

        private IConfigurationRoot BuildConfiguration()
        {
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appconfig.json", true)
                .AddEnvironmentVariables()
                .AddAzureKeyVaultConfiguration(Constants.KeyVaultName);

            return configBuilder.Build();
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
