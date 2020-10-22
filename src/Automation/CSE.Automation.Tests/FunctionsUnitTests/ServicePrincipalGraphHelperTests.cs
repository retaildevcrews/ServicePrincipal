using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.KeyVault;
using CSE.Automation.Model;
using CSE.Automation.Processors;
using CSE.Automation.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CSE.Automation.Tests.FunctionsUnitTests
{
    public class ServicePrincipalGraphHelperTests
    {
        private IHost _host;

        public ServicePrincipalGraphHelperTests()
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
            //var secretServiceSettings = new SecretServiceSettings() { KeyVaultName = config[Constants.KeyVaultName] };
            //var credServiceSettings = new CredentialServiceSettings() { AuthType = config[Constants.AuthType].As<AuthenticationType>() };

            return
                //Host.CreateDefaultBuilder(args)
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
                        //.AddSingleton(credServiceSettings)
                        //.AddSingleton(secretServiceSettings)

                        .AddTransient<GraphHelperSettings>()
                        .AddScoped<IGraphHelper<ServicePrincipal>, ServicePrincipalGraphHelper>();

                    //services.AddSingleton<IRevolutionMapperConfiguration, RevolutionMapperConfiguration>(provider =>
                    //{
                    //    var configuration = new MapperConfiguration(cfg =>
                    //    {
                    //        cfg.AddProfile<RevolutionFHIRModelProfile>();
                    //    });
                    //    configuration.CompileMappings();
                    //    return new RevolutionMapperConfiguration() { Provider = configuration };
                    //});
                });

        }

        ILogger<ServicePrincipalGraphHelper> GetLogger(IServiceScope scope)
        {
            return scope.ServiceProvider.GetService<ILogger<ServicePrincipalGraphHelper>>();
        }
        ProcessorConfiguration GetConfiguration()
        {
            var contents = @"{
  ""id"": ""02a54ac9 - 441e-43f1 - 88ee - fde420db2559"",
  ""filterString"": ""filterstring"",
  ""selectFields"": [
    ""appId"",
    ""displayName"",
    ""notes"",
    ""owners"",
    ""notificationEmailAddresses""
  ],
  ""deltaLink"": """",
  ""runState"": ""seedAndRun"",
  ""lastDeltaRun"": """",
  ""lastSeedTime"": """",
  ""name"": ""ServicePrincipal Processor"",
  ""description"": ""Descriptive Text"",
  ""configType"": ""ServicePrincipal""
}";
            return JsonConvert.DeserializeObject<ProcessorConfiguration>(contents);
        }


        [Fact]
        public async Task GetDeltaGraphObjects_GetAll()
        {
            using (var serviceScope = _host.Services.CreateScope())
            {
                var config = GetConfiguration();
                var service = serviceScope.ServiceProvider.GetService<IGraphHelper<ServicePrincipal>>();

                var results = await service.GetDeltaGraphObjects(config);

            }
            Assert.True(true);
        }
    }
}
