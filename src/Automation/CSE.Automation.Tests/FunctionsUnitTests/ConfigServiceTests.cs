using CSE.Automation.DataAccess;
using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.KeyVault;
using CSE.Automation.Model;
using CSE.Automation.Properties;
using CSE.Automation.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;

namespace CSE.Automation.Tests.FunctionsUnitTests
{
    public class ConfigServiceTests
    {
        private IHost _host;

        public ConfigServiceTests()
        {
            Initialize();
        }

        void Initialize()
        {
            var builder = CreateHostBuilder(null);
            _host = builder.Build();
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
            using (var serviceScope = _host.Services.CreateScope())
            {
                var configService = serviceScope.ServiceProvider.GetService<ConfigService>();

                byte[] defaultConfigurationResource = Resources.ServicePrincipalProcessorConfiguration;

                var testProcessorConfiguration = configService.Get("02a54ac9-441e-43f1-88ee-fde420db2559", ProcessorType.ServicePrincipal, "ServicePrincipalProcessorConfiguration");
                string originalDescription = testProcessorConfiguration.Description;
                testProcessorConfiguration.Description = "Test Value";

                await configService.Put(testProcessorConfiguration);

                Assert.True(originalDescription == "Descriptive Text");

                var updatedProcessorConfiguration = configService.Get("02a54ac9-441e-43f1-88ee-fde420db2559", ProcessorType.ServicePrincipal, "ServicePrincipalProcessorConfiguration");
                string updatedDescription = updatedProcessorConfiguration.Description;
                updatedProcessorConfiguration.Description = originalDescription;

                await configService.Put(updatedProcessorConfiguration);

                Assert.True(updatedDescription == "Test Value");
            }
        }
    }
}
