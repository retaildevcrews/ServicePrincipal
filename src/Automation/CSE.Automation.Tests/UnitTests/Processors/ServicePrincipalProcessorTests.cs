using System;
using System.Diagnostics;
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
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using ActivityContext = CSE.Automation.Model.ActivityContext;

namespace CSE.Automation.Tests.UnitTests.Processors
{
    public class ServicePrincipalProcessorTests : IClassFixture<UnitTestFixture>
    {
        private UnitTestFixture fixture;
        private readonly ITestOutputHelper output;

        public ServicePrincipalProcessorTests(UnitTestFixture fixture, ITestOutputHelper output)
        {
            this.fixture = fixture;
            this.output = output;

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


            services
                .AddSingleton<IServicePrincipalProcessorSettings, ServicePrincipalProcessorSettingsMock>()
                .AddSingleton<IAuditRepository, DefaultAuditRepository>()
                .AddSingleton<IQueueServiceFactory, DefaultQueueServiceFactory<ServicePrincipalUpdateCommand>>()
                .AddSingleton<IConfigService<ProcessorConfiguration>, DefaultConfigService>()
                .AddSingleton<IObjectTrackingService, ObjectTrackingServiceMock>()
                .AddSingleton<IModelValidatorFactory, ModelValidatorFactory>()
                .AddSingleton<IGraphHelper<User>>(new UserGraphHelperMock() { Data = testUsers.Select(x => new User { Id = x }).ToList() })

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
        /// <param name="testData">An instance of test data returned from the class data generator</param>
        /// <remarks>
        ///     Dependencies:
        ///         IQueueService - mocked, no messages should be posted to Update queue
        ///         IAuditService - mocked, we check for PASS audit messages and no other audit messages
        ///         IObjectService - mocked, we check that LKG is either created or updated
        /// </remarks>
        [Theory]
        [Trait("Category", "Unit")]
        [ClassData(typeof(EvaluateServicePrincipalPassTestData))]
        public void Evaluate_should_pass(ServicePrincipalTestData testData)
        {
            Evaluate_Test_Data(testData, AuditActionType.Pass);
        }

        /// <summary>
        /// Evaluation tests that should pass
        /// </summary>
        /// <param name="testData">An instance of test data returned from the class data generator</param>
        /// <remarks>
        ///     Dependencies:
        ///         IQueueService - mocked, no messages should be posted to Update queue
        ///         IAuditService - mocked, we check for PASS audit messages and no other audit messages
        ///         IObjectService - mocked, we check that LKG is either created or updated
        /// </remarks>
        [Theory]
        [Trait("Category", "Unit")]
        [ClassData(typeof(EvaluateServicePrincipalFailTestData))]
        public void Evaluate_should_fail(ServicePrincipalTestData testData)
        {
            Evaluate_Test_Data(testData, AuditActionType.Fail);
        }

        /// <summary>
        /// Perform an Evaluate test on the data record.
        /// </summary>
        /// <param name="testData"></param>
        /// <param name="auditActionType">The expected AuditActionType of all audit messages in the audit repository.</param>
        private void Evaluate_Test_Data(ServicePrincipalTestData testData, AuditActionType auditActionType)
        {
            // Setup LKG data
            var objectService = fixture.Host.Services.GetService<IObjectTrackingService>() as ObjectTrackingServiceMock;
            Assert.NotNull(objectService);
            objectService.WithData(testData.ObjectServiceData);

            var auditRepository = fixture.Host.Services.GetService<IAuditRepository>() as DefaultAuditRepository;
            Assert.NotNull(auditRepository);

            var queueService = fixture.Host.Services.GetService<IQueueServiceFactory>()?.Create(null, null) as DefaultAzureQueueService<ServicePrincipalUpdateCommand>;
            Assert.NotNull(queueService);

            var processor = fixture.Host.Services.GetService<IServicePrincipalProcessor>();
            Assert.NotNull(processor);

            var context = new ActivityContext(null);

            processor.Evaluate(context, testData.Model);

            // Assertions
            Assert.True(auditRepository.Data.Count == testData.ExpectedAuditCodes.Length, $"Audit Item Count expected: {testData.ExpectedAuditCodes.Length} actual: {auditRepository.Data.Count}");

            for (var index = 0; index < auditRepository.Data.Count; index++)
            {
                var auditItem = auditRepository.Data[index];
                output.WriteLine($"Audit Item {index+1}");
                output.WriteLine(JsonConvert.SerializeObject(auditItem, Formatting.Indented));

                Assert.Equal(auditActionType, auditItem.Type);
                Assert.Equal(testData.ExpectedAuditCodes[index], auditItem.Code);
                Assert.Equal(DateTime.Now.ToString("yyyyMM"), auditItem.AuditYearMonth);

                Assert.Equal(context.CorrelationId, auditItem.Descriptor.CorrelationId);
                Assert.Equal(testData.Model.Id, auditItem.Descriptor.ObjectId);
                Assert.Equal(testData.Model.AppId, auditItem.Descriptor.AppId);
                Assert.Equal(testData.Model.DisplayName, auditItem.Descriptor.DisplayName);
            }

            output.WriteLine("");
            if (testData.ExpectedUpdateMessage == null)
            {
                Assert.True(queueService.Data.Count == 0);
                output.WriteLine($"No Queue Messages");
            }
            else 
            {
                var queueMessage = queueService.Data.FirstOrDefault();
                Assert.NotNull(queueMessage);

                var command = queueMessage.Document;
                Assert.NotNull(command);
                output.WriteLine($"QueueMessage Command");
                output.WriteLine(JsonConvert.SerializeObject(command, Formatting.Indented));

                Assert.Equal(command.CorrelationId, context.CorrelationId);
                Assert.Equal(testData.ExpectedUpdateMessage.Action, command.Action);
                Assert.Equal(testData.ExpectedUpdateMessage.Notes, command.Notes);
            }
        }
    }
}
