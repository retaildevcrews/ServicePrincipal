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
        private IHost host;

        public ServicePrincipalGraphHelperTests()
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
            var page = Substitute.For<IServicePrincipalDeltaCollectionPage>();
            IServicePrincipalDeltaRequest temp = null;
            page.NextPageRequest.Returns(temp);
            Task<IServicePrincipalDeltaCollectionPage> results = Task.FromResult(page);
            var graphClient = Substitute.For<IGraphServiceClient>();
            graphClient.ServicePrincipals.Delta().Request().Filter(Arg.Any<String>()).GetAsync().Returns(results);

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
                        .AddSingleton<ISecretClient>(Substitute.For<ISecretClient>())
                        .AddSingleton<IAuditService>(Substitute.For<IAuditService>())
                        .AddSingleton<IGraphServiceClient>(graphClient)
                        .AddTransient<GraphHelperSettings>()
                        .AddScoped<IGraphHelper<ServicePrincipal>, ServicePrincipalGraphHelper>();
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
  ""selectFields"": [
    ""appId"",
    ""displayName"",
    ""notes"",
    ""owners"",
    ""notificationEmailAddresses""
  ],
  ""deltaLink"": """",
  ""runState"": ""seed"",
  ""lastDeltaRun"": """",
  ""lastSeedTime"": """",
  ""description"": """",
  ""configType"": ""ServicePrincipal""
}";
            return JsonConvert.DeserializeObject<ProcessorConfiguration>(contents);
        }


        [Fact]
        [Trait("Category","Unit")]
        public void GetDeltaGraphObjects_GetAll()
        {
            using var serviceScope = host.Services.CreateScope();

            var config = GetConfiguration();

            var service = serviceScope.ServiceProvider.GetService<IGraphHelper<ServicePrincipal>>();

            var task = Task.Run(() =>
            {
                return service.GetDeltaGraphObjects(new ActivityContext(null), config);
            });
            
            if (!task.Wait(TimeSpan.FromSeconds(2)))
            {
                throw new TimeoutException("Test Not Mocked Properly May Lead to Infinite Loop");
            }
        }
    }
}
