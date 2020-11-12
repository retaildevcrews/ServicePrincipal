using CSE.Automation.DataAccess;
using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.KeyVault;
using CSE.Automation.Model;
using CSE.Automation.Properties;
using CSE.Automation.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Xunit;

namespace CSE.Automation.Tests.FunctionsUnitTests
{
    public class ConfigServiceTests
    {
        private IHost host;

        public ConfigServiceTests()
        {
            Initialize();
        }

        void Initialize()
        {
            var builder = CreateHostBuilder(null);
            host = builder.Build();
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            // SERVICES SETTINGS
            var secretServiceSettings = new SecretServiceSettings() { KeyVaultName = "sp-kv-dev" };
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
                                Uri = "https://sp-cosmosa-dev.documents.azure.com:443/",
                                Key = secretService.GetSecretValue(Constants.CosmosDBKeyName),
                                DatabaseName = "sp-cosmos-dev",
                                CollectionName = "Configuration"
                            })
                        .AddScoped<ConfigService>();
                });

        }

        ILogger<ConfigService> GetLogger(IServiceScope scope)
        {
            return scope.ServiceProvider.GetService<ILogger<ConfigService>>();
        }

        [Fact]
        public async Task TestConfigServiceReadWriteAsync()
        {
            using (var serviceScope = host.Services.CreateScope())
            {
                var configService = serviceScope.ServiceProvider.GetService<ConfigService>();

                byte[] defaultConfigurationResource = Resources.ServicePrincipalProcessorConfiguration;
                var initalDocumentAsString = System.Text.Encoding.Default.GetString(defaultConfigurationResource);
                ProcessorConfiguration defaultConfiguration = JsonConvert.DeserializeObject<ProcessorConfiguration>(initalDocumentAsString);

                var testProcessorConfiguration = configService.Get(defaultConfiguration.Id, ProcessorType.ServicePrincipal, "ServicePrincipalProcessorConfiguration");
                string originalDescription = testProcessorConfiguration.Description;
                testProcessorConfiguration.Description = "Test Value";

                await configService.Put(testProcessorConfiguration);

                Assert.True(originalDescription == "Descriptive Text");

                var updatedProcessorConfiguration = configService.Get(defaultConfiguration.Id, ProcessorType.ServicePrincipal, "ServicePrincipalProcessorConfiguration");
                string updatedDescription = updatedProcessorConfiguration.Description;
                updatedProcessorConfiguration.Description = originalDescription;

                await configService.Put(updatedProcessorConfiguration);

                Assert.True(updatedDescription == "Test Value");
            }
        }

        [Fact]
        public void TestConfigServiceConfigNotFoundAsync()
        {
            using (var serviceScope = host.Services.CreateScope())
            {
                var configService = serviceScope.ServiceProvider.GetService<ConfigService>();

                byte[] defaultConfigurationResource = Resources.ServicePrincipalProcessorConfiguration;

                var testProcessorConfiguration1 = configService.Get("new-pr0cess0r-c0nfig", ProcessorType.ServicePrincipal, "ServicePrincipalProcessorConfiguration");
                Assert.True(testProcessorConfiguration1 == null);
            }
        }

        [Fact]
        public async Task TestConfigServiceConfigNewAsync()
        {
            using (var serviceScope = host.Services.CreateScope())
            {
                var configService = serviceScope.ServiceProvider.GetService<ConfigService>();

                byte[] defaultConfigurationResource = Resources.ServicePrincipalProcessorConfiguration;

                var testProcessorConfiguration2 = configService.Get("new-pr0cess0r-c0nf1g", ProcessorType.ServicePrincipal, "ServicePrincipalProcessorConfiguration", true);
                Assert.True(testProcessorConfiguration2.Id == "new-pr0cess0r-c0nf1g");

                var configRepositoryService = serviceScope.ServiceProvider.GetService<ConfigRepository>();

                ProcessorConfiguration item = await configRepositoryService.DeleteDocumentAsync("new-pr0cess0r-c0nf1g", "ServicePrincipal");
            }
        }
    }
}
