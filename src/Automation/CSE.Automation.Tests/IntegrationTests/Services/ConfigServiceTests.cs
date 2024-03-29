using System.IO;
using System.Threading.Tasks;
using CSE.Automation.DataAccess;
using CSE.Automation.Extensions;
using CSE.Automation.Interfaces;
using CSE.Automation.KeyVault;
using CSE.Automation.Model;
using CSE.Automation.Processors;
using CSE.Automation.Properties;
using CSE.Automation.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xunit;

namespace CSE.Automation.Tests.IntegrationTests.Services
{
    public class ConfigServiceTests
    {
        private IHost host;
        private IConfigurationRoot _config;

        public ConfigServiceTests()
        {
            Initialize();
        }

        void Initialize()
        {
            var builder = CreateHostBuilder(null);
            host = builder.Build();
        }

        IHostBuilder CreateHostBuilder(string[] args)
        {
            BuildConfiguration();

            // SERVICES SETTINGS
            var secretServiceSettings = new SecretServiceSettings() { KeyVaultName = _config[Constants.KeyVaultName] };
            var credServiceSettings = new CredentialServiceSettings() { AuthType = AuthenticationType.CLI };

            var credService = new CredentialService(credServiceSettings);
            var secretService = new SecretService(secretServiceSettings,credService) ;

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
                    services
                        .AddSingleton(credServiceSettings)
                        .AddSingleton(secretServiceSettings)
                        .AddSingleton(credService)
                        .AddSingleton(secretService)
                        .AddSingleton<ICredentialService>(credService)
                        .AddSingleton<ISecretClient>(secretService)
                        .AddSingleton<ConfigRepository>()
                        .AddSingleton<IConfigRepository, ConfigRepository>(provider => provider.GetRequiredService<ConfigRepository>())
                        .AddSingleton<ConfigRespositorySettings>(x => new ConfigRespositorySettings(x.GetRequiredService<ISecretClient>())
                            {
                                Uri = _config[Constants.CosmosDBURLName],
                                Key = _config[Constants.CosmosDBKeyName],
                                DatabaseName = _config[Constants.CosmosDBDatabaseName],
                                CollectionName = _config[Constants.CosmosDBConfigCollectionName]
                            })
                        .AddScoped<ConfigService>();
                });

        }

        private void BuildConfiguration()
        {
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appconfig.json", true)
                .AddEnvironmentVariables()
                .AddAzureKeyVaultConfiguration(Constants.KeyVaultName);

            _config = configBuilder.Build();
        }

        ILogger<ConfigService> GetLogger(IServiceScope scope)
        {
            return scope.ServiceProvider.GetService<ILogger<ConfigService>>();
        }

        [Fact]
        [Trait("Category","Integration")]
        public async Task TestConfigServiceReadWriteAsync()
        {
            using var serviceScope = host.Services.CreateScope();
            var configService = serviceScope.ServiceProvider.GetService<ConfigService>();

            byte[] defaultConfigurationResource = Resources.ServicePrincipalProcessorConfiguration;
            var initialDocumentAsString = System.Text.Encoding.Default.GetString(defaultConfigurationResource);
            ProcessorConfiguration defaultConfiguration = JsonConvert.DeserializeObject<ProcessorConfiguration>(initialDocumentAsString);

            var config = configService.Get(defaultConfiguration.Id, ProcessorType.ServicePrincipal, ServicePrincipalProcessor.ConstDefaultConfigurationResourceName);
            string originalDescription = config.Description;
            config.Description = "Test Value";

            await configService.Put(config);

            var updatedConfig = configService.Get(defaultConfiguration.Id, ProcessorType.ServicePrincipal, ServicePrincipalProcessor.ConstDefaultConfigurationResourceName);

            var repository = serviceScope.ServiceProvider.GetService<ConfigRepository>();
            var item = await repository.DeleteDocumentAsync(config.Id, config.ConfigType.ToString());

            Assert.True(originalDescription == "");
            Assert.True(updatedConfig.Description == "Test Value");
        }

        [Fact]
        [Trait("Category","Integration")]
        public void TestConfigServiceConfigNotFound_NoCreate_Async()
        {
            using var serviceScope = host.Services.CreateScope();
            var configService = serviceScope.ServiceProvider.GetService<ConfigService>();

            byte[] defaultConfigurationResource = Resources.ServicePrincipalProcessorConfiguration;

            var configName = Get8CharacterRandomString();
            var config = configService.Get(configName, ProcessorType.ServicePrincipal, ServicePrincipalProcessor.ConstDefaultConfigurationResourceName, createIfNotFound: false);
            Assert.True(config == null);
        }

        [Fact]
        [Trait("Category","Integration")]
        public async Task TestConfigServiceConfigNotFound_Create_Async()
        {
            using var serviceScope = host.Services.CreateScope();
            var configService = serviceScope.ServiceProvider.GetService<ConfigService>();

            byte[] defaultConfigurationResource = Resources.ServicePrincipalProcessorConfiguration;

            var configName = Get8CharacterRandomString();
            var config = configService.Get(configName, ProcessorType.ServicePrincipal, ServicePrincipalProcessor.ConstDefaultConfigurationResourceName, createIfNotFound:true);
            Assert.True(config.Id == configName);

            var repository = serviceScope.ServiceProvider.GetService<ConfigRepository>();

            var item = await repository.DeleteDocumentAsync(configName, config.ConfigType.ToString());
        }

        public static string Get8CharacterRandomString()
        {
            string path = Path.GetRandomFileName();
            path = path.Replace(".", ""); // Remove period.
            return path.Substring(0, 8);  // Return 8 character string
        }
    }
}
