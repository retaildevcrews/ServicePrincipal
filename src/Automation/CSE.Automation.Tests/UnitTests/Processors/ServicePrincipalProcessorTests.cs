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
                "user2@mydirectory.com",
                "LKG1@mydirectory.com",
                "LKG2@mydirectory.com"
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
                .AddSingleton<IQueueServiceFactory, DefaultQueueServiceFactory<ServicePrincipalUpdateCommand>>()
                .AddSingleton<IConfigService<ProcessorConfiguration>, DefaultConfigService>()
                .AddSingleton<IObjectTrackingService>(objectTrackingService)
                .AddSingleton<IModelValidatorFactory, ModelValidatorFactory>()
                .AddSingleton<IGraphHelper<User>>(new UserGraphHelperMock() { Data = testUsers.Select(x => new User{ Id = x }).ToList() })

                .AddScoped<IActivityService, NoopActivityService>()
                .AddScoped<IAuditService, AuditService>()
                .AddScoped<IServicePrincipalProcessor, ServicePrincipalProcessor>()
                .AddScoped<IServicePrincipalGraphHelper, NoopServicePrincipalGraphHelper>()
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
            Assert.NotNull(auditRepository);

            var processor = fixture.Host.Services.GetService<IServicePrincipalProcessor>();
            Assert.NotNull(processor);

            var context = new ActivityContext(null);

            processor.Evaluate(context, entity);

            // Assertions
            Assert.True(auditRepository.Data.Count == 1);

            var auditItem = auditRepository.Data[0];
            Assert.Equal(AuditActionType.Pass, auditItem.Type);
            Assert.Equal(AuditCode.Pass, auditItem.Code);
            Assert.Equal(DateTime.Now.ToString("yyyyMM"), auditItem.AuditYearMonth);

            Assert.Equal(context.CorrelationId, auditItem.Descriptor.CorrelationId);
            Assert.Equal(entity.Id, auditItem.Descriptor.ObjectId);
            Assert.Equal(entity.AppId, auditItem.Descriptor.AppId);
            Assert.Equal(entity.DisplayName, auditItem.Descriptor.DisplayName);
        }

        /// <summary>
        /// Evaluation tests that should pass
        /// </summary>
        /// <param name="testModel">An instance of test data returned from the class data generator</param>
        /// <remarks>
        ///     Dependencies:
        ///         IQueueService - mocked, no messages should be posted to Update queue
        ///         IAuditService - mocked, we check for PASS audit messages and no other audit messages
        ///         IObjectService - mocked, we check that LKG is either created or updated
        /// </remarks>
        [Theory]
        [Trait("Category", "Unit")]
        [ClassData(typeof(EvaluateServicePrincipalFailTestData))]
        public void Evaluate_should_fail(ServicePrincipalTestData testModel)
        {
            // Setup LKG data
            var objectService = fixture.Host.Services.GetService<IObjectTrackingService>() as ObjectTrackingServiceMock;
            Assert.NotNull(objectService);
            objectService.WithData(testModel.ObjectServiceData);

            var auditRepository = fixture.Host.Services.GetService<IAuditRepository>() as DefaultAuditRepository;
            Assert.NotNull(auditRepository);

            var queueService = fixture.Host.Services.GetService<IQueueServiceFactory>()?.Create(null, null) as DefaultAzureQueueService<ServicePrincipalUpdateCommand>;
            Assert.NotNull(queueService);

            var processor = fixture.Host.Services.GetService<IServicePrincipalProcessor>();
            Assert.NotNull(processor);

            var context = new ActivityContext(null);

            processor.Evaluate(context, testModel.Model);

            // Assertions
            Assert.True(auditRepository.Data.Count == testModel.ExpectedAuditCodes.Length, $"Audit Item Count == {auditRepository.Data.Count}");

            for (var index = 0; index < auditRepository.Data.Count; index++)
            {
                var auditItem = auditRepository.Data[index];

                Assert.Equal(AuditActionType.Fail, auditItem.Type);
                Assert.Equal(testModel.ExpectedAuditCodes[index], auditItem.Code);
                Assert.Equal(DateTime.Now.ToString("yyyyMM"), auditItem.AuditYearMonth);

                Assert.Equal(context.CorrelationId, auditItem.Descriptor.CorrelationId);
                Assert.Equal(testModel.Model.Id, auditItem.Descriptor.ObjectId);
                Assert.Equal(testModel.Model.AppId, auditItem.Descriptor.AppId);
                Assert.Equal(testModel.Model.DisplayName, auditItem.Descriptor.DisplayName);

                if (testModel.ExpectedUpdateMessage != null)
                {
                    var queueMessage = queueService.Data.FirstOrDefault();
                    Assert.NotNull(queueMessage);

                    var command = queueMessage.Document;
                    Assert.NotNull(command);

                    Assert.Equal(command.CorrelationId, context.CorrelationId);
                    Assert.Equal(ServicePrincipalUpdateAction.Update, command.Action);
                    Assert.Equal(testModel.ExpectedUpdateMessage.Notes, command.Notes);
                }
            }
        }
    }
}
