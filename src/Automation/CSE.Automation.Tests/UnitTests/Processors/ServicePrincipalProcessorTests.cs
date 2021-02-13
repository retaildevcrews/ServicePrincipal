using System;
using System.Linq;
using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Model.Validators;
using CSE.Automation.Processors;
using CSE.Automation.Services;
using CSE.Automation.Tests.Fixtures;
using CSE.Automation.Tests.Mocks;
using CSE.Automation.Tests.TestDataGenerators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Xunit;

namespace CSE.Automation.Tests.UnitTests.Processors
{
    public class ServicePrincipalProcessorTests : IClassFixture<UnitTestFixture>
    {
        private UnitTestFixture fixture;
        public ServicePrincipalProcessorTests(UnitTestFixture fixture)
        {
            this.fixture = fixture;
            this.fixture.BuildHost(() => null, RegisterServices);
        }

        private void RegisterServices(IConfigurationRoot configRoot, IServiceCollection services)
        {
            /*
            ServicePrincipalProcessorSettings settings,
            IServicePrincipalGraphHelper graphHelper,
            IQueueServiceFactory queueServiceFactory,
            IConfigService< ProcessorConfiguration > configService,
            IObjectTrackingService objectService,
            IAuditService auditService,
            IActivityService activityService,
            IModelValidatorFactory modelValidatorFactory,
            ILogger<ServicePrincipalProcessor> logger)
            */

            var testUsers = new string[]
            {
                "user1@mydirectory.com",
                "user2@mydirectory.com"
            };

            var objectTrackingService = ObjectTrackingServiceMock.Create().WithData(new[]
            {
                new TrackingModel
                {
                    Id = "LKG1",
                    CorrelationId = "Correlation 1",
                    Created = DateTimeOffset.Now,
                    LastUpdated = DateTimeOffset.Now,
                    ObjectType = ObjectType.ServicePrincipal,
                    Entity = new ServicePrincipalModel
                    {
                        Id = "LKG1"
                    }
                }
            });

            services
                .AddSingleton<IServicePrincipalProcessorSettings, ServicePrincipalProcessorSettingsMock>()
                .AddSingleton<IAuditRepository, DefaultAuditRepository>()
                .AddSingleton<IQueueServiceFactory, NoopQueueServiceFactory>()
                .AddSingleton<IConfigService<ProcessorConfiguration>, DefaultConfigService>()
                .AddSingleton<IObjectTrackingService>(objectTrackingService)
                .AddSingleton<IModelValidatorFactory, ModelValidatorFactory>()
                .AddSingleton<IGraphHelper<User>>(new UserGraphHelperMock() { Data = testUsers.Select(x => new User{ Id = x }).ToList() })

                .AddScoped<IActivityService, NoopActivityService>()
                .AddScoped<IAuditService, AuditService>()
                .AddScoped<IServicePrincipalProcessor, ServicePrincipalProcessor>()
                .AddScoped<IServicePrincipalGraphHelper, ServicePrincipalGraphHelperMock>()
                .AddScoped<ConfigService>()
                .AddScoped<IModelValidator<GraphModel>, GraphModelValidator>()
                .AddScoped<IModelValidator<ServicePrincipalModel>, ServicePrincipalModelValidator>()
                .AddScoped<IModelValidator<AuditEntry>, AuditEntryValidator>();
        }

        
        /// <summary>
        /// Evaluation tests that should pass
        /// </summary>
        /// <param name="entity">An instance returned from the class data generator</param>
        /// <remarks>
        ///     Dependencies:
        ///         IQueueService - mocked, no messages should be posted to Update queue
        ///         IAuditService - mocked, we check for PASS audit messages and no other audit messages
        ///         IObjectService - mocked, we check that LKG is either created or updated
        /// </remarks>
        [Theory]
        [Trait("Category", "Unit")]
        [ClassData(typeof(EvaluateServicePrincipalPassTestData))]
        public void Evaluate_should_pass(ServicePrincipalModel entity)
        {
            var auditRepository = fixture.Host.Services.GetService<IAuditRepository>() as DefaultAuditRepository;
            var processor = fixture.Host.Services.GetService<IServicePrincipalProcessor>();
            var context = new ActivityContext(null);

            processor.Evaluate(context, entity);

            // Assertions
            Assert.True(auditRepository != null);
            Assert.True(auditRepository.Data.Count == 1);
        }
    }
}
