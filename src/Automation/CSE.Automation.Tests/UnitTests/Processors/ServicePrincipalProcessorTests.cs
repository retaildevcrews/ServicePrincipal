using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Model.Commands;
using CSE.Automation.Model.Validators;
using CSE.Automation.Processors;
using CSE.Automation.Services;
using CSE.Automation.Tests.Mocks;
using CSE.Automation.Tests.Mocks.Graph;
using CSE.Automation.Tests.TestDataGenerators;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Services.AppAuthentication;
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
        private readonly UnitTestFixture fixture;
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
                .AddSingleton<IGraphHelperSettings, GraphHelperSettingsMock>()

                .AddSingleton<IGraphServiceClient, GraphServiceClientMock>()
                .AddSingleton<IServicePrincipalGraphHelper, ServicePrincipalGraphHelper>()
                .AddSingleton<IAuditRepository, AuditRepositoryMock>()
                .AddSingleton<IQueueServiceFactory, MockQueueServiceFactory>()
                .AddSingleton<IConfigService<ProcessorConfiguration>, ConfigServiceMock>()
                .AddSingleton<IObjectTrackingService, ObjectTrackingServiceMock>()
                .AddSingleton<IModelValidatorFactory, ModelValidatorFactory>()
                .AddSingleton<IGraphHelper<User>>(new UserGraphHelperMock() { Data = testUsers.Select(x => new User { Id = x }).ToList() })

                .AddScoped<IActivityService, DefaultActivityService>()
                .AddScoped<IAuditService, AuditService>()
                .AddScoped<IServicePrincipalProcessor, ServicePrincipalProcessor>()
                //.AddScoped<IServicePrincipalGraphHelper, NoopServicePrincipalGraphHelper>()
                .AddScoped<ConfigService>()
                .AddScoped<IModelValidator<GraphModel>, GraphModelValidator>()
                .AddScoped<IModelValidator<ServicePrincipalModel>, ServicePrincipalModelValidator>()
                .AddScoped<IModelValidator<AuditEntry>, AuditEntryValidator>();
        }

        /// <summary>
        /// Evaluation tests that should pass
        /// </summary>
        /// <param name="evaluateTestData">An instance of test data returned from the class data generator</param>
        /// <remarks>
        ///     Dependencies:
        ///         IQueueService - mocked, no messages should be posted to Update queue
        ///         IAuditService - mocked, we check for PASS audit messages and no other audit messages
        ///         IObjectService - mocked, we check that LKG is either created or updated
        /// </remarks>
        [Theory]
        [Trait("Category", "Unit")]
        [ClassData(typeof(DiscoverServicePrincipalTestDataGenerator))]
        public async Task Discover_should_pass(ServicePrincipalDiscoverTestData testData)
        {
            try
            {
                await Discover(testData).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Assert.True(false, e.Message);
            }
        }


        /// <summary>
        /// Evaluation tests that should pass
        /// </summary>
        /// <param name="evaluateTestData">An instance of test data returned from the class data generator</param>
        /// <remarks>
        ///     Dependencies:
        ///         IQueueService - mocked, no messages should be posted to Update queue
        ///         IAuditService - mocked, we check for PASS audit messages and no other audit messages
        ///         IObjectService - mocked, we check that LKG is either created or updated
        /// </remarks>
        [Theory]
        [Trait("Category", "Unit")]
        [ClassData(typeof(EvaluateServicePrincipalPassTestDataGenerator))]
        public async Task Evaluate_should_pass(ServicePrincipalEvaluateTestData evaluateTestData)
        {
            try
            {
                await Evaluate(evaluateTestData, AuditActionType.Pass).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Assert.True(false, e.Message);
            }

        }

        /// <summary>
        /// Evaluation tests that should fail
        /// </summary>
        /// <param name="evaluateTestData">An instance of test data returned from the class data generator</param>
        /// <remarks>
        ///     Dependencies:
        ///         IQueueService - mocked, no messages should be posted to Update queue
        ///         IAuditService - mocked, we check for PASS audit messages and no other audit messages
        ///         IObjectService - mocked, we check that LKG is either created or updated
        /// </remarks>
        [Theory]
        [Trait("Category", "Unit")]
        [ClassData(typeof(EvaluateServicePrincipalFailTestDataGenerator))]
        public async Task Evaluate_should_fail(ServicePrincipalEvaluateTestData evaluateTestData)
        {
            try
            {
                await Evaluate(evaluateTestData, AuditActionType.Fail).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Assert.True(false, e.Message);
            }

        }

        //[Theory]
        //[Trait("Category", "Unit")]
        //[ClassData(typeof(UpdateServicePrincipalTestDataGenerator))]
        //public async Task Update_should_pass(ServicePrincipalUpdateTestData testData)
        //{
        //    try
        //    {
        //        await Update(testData).ConfigureAwait(false);
        //    }
        //    catch (Exception e)
        //    {
        //        Assert.True(false, e.Message);
        //    }
        //}

        /// <summary>
        /// Perform an Evaluate test on the data record.
        /// </summary>
        /// <param name="testData"></param>
        /// <param name="auditActionType">The expected AuditActionType of all audit messages in the audit repository.</param>
        private async Task Evaluate(ServicePrincipalEvaluateTestData testData, AuditActionType auditActionType)
        {
            // Setup LKG data
            var objectService = fixture.Host.Services.GetService<IObjectTrackingService>() as ObjectTrackingServiceMock;
            Assert.NotNull(objectService);
            objectService.WithData(testData.ObjectServiceData);

            var auditRepository = fixture.Host.Services.GetService<IAuditRepository>() as AuditRepositoryMock;
            Assert.NotNull(auditRepository);

            var queueService = fixture.Host.Services.GetService<IQueueServiceFactory>()?.Create(null, "update") as AzureQueueServiceMock<ServicePrincipalUpdateCommand>;
            Assert.NotNull(queueService);

            var processor = fixture.Host.Services.GetService<IServicePrincipalProcessor>();
            Assert.NotNull(processor);

            var activityService = fixture.Host.Services.GetService<IActivityService>() as DefaultActivityService;
            Assert.NotNull(activityService);
            var context = activityService.CreateContext("Test Case");

            await processor.Evaluate(context, testData.Target).ConfigureAwait(false);

            // Assertions

            // AUDIT
            Assert.True(auditRepository.Data.Count == testData.ExpectedAuditCodes.Length, $"Audit Item Count expected: {testData.ExpectedAuditCodes.Length} actual: {auditRepository.Data.Count}");

            for (var index = 0; index < auditRepository.Data.Count; index++)
            {
                var auditItem = auditRepository.Data[index];
                output.WriteLine($"Audit Item {index + 1}");
                output.WriteLine(JsonConvert.SerializeObject(auditItem, Formatting.Indented));

                Assert.Equal(auditActionType, auditItem.Type);
                Assert.Equal(testData.ExpectedAuditCodes[index], auditItem.Code);
                Assert.Equal(DateTime.Now.ToString("yyyyMM"), auditItem.AuditYearMonth);

                Assert.Equal(context.CorrelationId, auditItem.Descriptor.CorrelationId);
                Assert.Equal(testData.Target.Id, auditItem.Descriptor.ObjectId);
                Assert.Equal(testData.Target.AppId, auditItem.Descriptor.AppId);
                Assert.Equal(testData.Target.DisplayName, auditItem.Descriptor.DisplayName);
            }

            // UPDATE QUEUE
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


        private async Task Update(ServicePrincipalUpdateTestData updateTestData)
        {
            // Setup LKG data
            var objectService = fixture.Host.Services.GetService<IObjectTrackingService>() as ObjectTrackingServiceMock;
            Assert.NotNull(objectService);
            objectService.WithData(updateTestData.InitialObjectServiceData);

            var auditRepository = fixture.Host.Services.GetService<IAuditRepository>() as AuditRepositoryMock;
            Assert.NotNull(auditRepository);

            var processor = fixture.Host.Services.GetService<IServicePrincipalProcessor>();
            Assert.NotNull(processor);

            var context = new ActivityContext(null);

            await processor.UpdateServicePrincipal(context, updateTestData.Target).ConfigureAwait(false);

            // Assertions

            // AUDIT
            Assert.True(auditRepository.Data.Count == updateTestData.ExpectedAuditCodes.Length, $"Audit Item Count expected: {updateTestData.ExpectedAuditCodes.Length} actual: {auditRepository.Data.Count}");

            for (var index = 0; index < auditRepository.Data.Count; index++)
            {
                var auditItem = auditRepository.Data[index];
                output.WriteLine($"Audit Item {index + 1}");
                output.WriteLine(JsonConvert.SerializeObject(auditItem, Formatting.Indented));

                Assert.Equal(AuditActionType.Change, auditItem.Type);
                Assert.Equal(updateTestData.ExpectedAuditCodes[index], auditItem.Code);
                Assert.Equal(DateTime.Now.ToString("yyyyMM"), auditItem.AuditYearMonth);

                Assert.Equal(context.CorrelationId, auditItem.Descriptor.CorrelationId);
                Assert.Equal(updateTestData.Target.Entity.Id, auditItem.Descriptor.ObjectId);
                Assert.Equal(updateTestData.Target.Entity.AppId, auditItem.Descriptor.AppId);
                Assert.Equal(updateTestData.Target.Entity.DisplayName, auditItem.Descriptor.DisplayName);
            }

            // LKG State
            Assert.True(updateTestData.ExpectedObjectServiceData.Length == objectService.Data.Count, $"ObjectTracking Item Count - expected: {updateTestData.ExpectedObjectServiceData.Length} actual: {objectService.Data.Count}");

            if (updateTestData.ExpectedObjectServiceData.Length > 0)
            {
                output.WriteLine("Output Object Tracking Service Data");
                var expectedValues = updateTestData.ExpectedObjectServiceData.ToDictionary(x => x.Id);
                foreach (var item in objectService.Data.Values)
                {
                    output.WriteLine(JsonConvert.SerializeObject(item, Formatting.Indented));
                    Assert.True(expectedValues.TryGetValue(item.Id, out var expectedItem), $"Failed to find {item.Id} in expected object service values.");

                    Assert.Equal(expectedItem.Id, item.Id);
                    //                    Assert.Equal(expectedItem.CorrelationId, );
                    Assert.Equal(expectedItem.ObjectType, item.ObjectType);

                    var model = TrackingModel.Unwrap<ServicePrincipalModel>(item);
                    var expectedModel = TrackingModel.Unwrap<ServicePrincipalModel>(expectedItem);


                }
            }
        }

        private async Task Discover(ServicePrincipalDiscoverTestData testData)
        {
            // Setup LKG data
            var objectService = fixture.Host.Services.GetService<IObjectTrackingService>() as ObjectTrackingServiceMock;
            Assert.NotNull(objectService);
            objectService.WithData(testData.InitialObjectServiceData);

            // Setup Graph data
            var graphServiceClientMock = fixture.Host.Services.GetService<IGraphServiceClient>() as GraphServiceClientMock;
            Assert.NotNull(graphServiceClientMock);
            graphServiceClientMock.WithData(testData.InitialServicePrincipals1, testData.InitialServicePrincipals2);

            var auditRepository = fixture.Host.Services.GetService<IAuditRepository>() as AuditRepositoryMock;
            Assert.NotNull(auditRepository);

            var queueService = fixture.Host.Services.GetService<IQueueServiceFactory>()?.Create(null, "evaluate") as AzureQueueServiceMock<ServicePrincipalEvaluateCommand>;
            Assert.NotNull(queueService);

            // Setup processor configuration
            var configService = fixture.Host.Services.GetService<IConfigService<ProcessorConfiguration>>() as ConfigServiceMock;
            Assert.NotNull(configService);
            configService.Config = new ProcessorConfiguration { };

            var processor = fixture.Host.Services.GetService<IServicePrincipalProcessor>();
            Assert.NotNull(processor);

            var activityService = fixture.Host.Services.GetService<IActivityService>() as DefaultActivityService;
            Assert.NotNull(activityService);
            var context = activityService.CreateContext("Test Case").WithCorrelationId(testData.Target.CorrelationId);

            await processor.DiscoverDeltas(context, testData.Target.DiscoveryMode == DiscoveryMode.FullSeed);

            // Assertions

            // AUDIT
            Assert.True(auditRepository.Data.Count == testData.ExpectedAuditCodes.Length, $"Audit Item Count expected: {testData.ExpectedAuditCodes.Length} actual: {auditRepository.Data.Count}");

            for (var index = 0; index < auditRepository.Data.Count; index++)
            {
                var auditItem = auditRepository.Data[index];
                output.WriteLine($"Audit Item {index + 1}");
                output.WriteLine(JsonConvert.SerializeObject(auditItem, Formatting.Indented));

                Assert.Equal(AuditActionType.Ignore, auditItem.Type);
                Assert.Equal(testData.ExpectedAuditCodes[index], auditItem.Code);
                Assert.Equal(DateTime.Now.ToString("yyyyMM"), auditItem.AuditYearMonth);

                Assert.Equal(context.CorrelationId, auditItem.Descriptor.CorrelationId);
            }

            // Queue
            Assert.Equal(testData.ExpectedEvaluateMessages, queueService.Data.Count);
            foreach (var message in queueService.Data)
            {
                var command = message.Document;
                Assert.NotNull(command);
                output.WriteLine($"QueueMessage Command");
                output.WriteLine(JsonConvert.SerializeObject(command, Formatting.Indented));

                Assert.Equal(command.CorrelationId, context.CorrelationId);
            }

            // LKG State
            Assert.True(testData.ExpectedObjectServiceData.Length == objectService.Data.Count, $"ObjectTracking Item Count - expected: {testData.ExpectedObjectServiceData.Length} actual: {objectService.Data.Count}");

            if (testData.ExpectedObjectServiceData.Length > 0)
            {
                output.WriteLine("Output Object Tracking Service Data");
                var expectedValues = testData.ExpectedObjectServiceData.ToDictionary(x => x.Id);
                foreach (var item in objectService.Data.Values)
                {
                    output.WriteLine(JsonConvert.SerializeObject(item, Formatting.Indented));
                    Assert.True(expectedValues.TryGetValue(item.Id, out var expectedItem), $"Failed to find {item.Id} in expected object service values.");

                    Assert.Equal(expectedItem.Id, item.Id);
                    Assert.Equal(expectedItem.CorrelationId, item.CorrelationId);
                    Assert.Equal(expectedItem.ObjectType, item.ObjectType);

                    // Check the TrackingModel wrapper
                    Assert.Equal(expectedItem.Id, item.Id);

                    var secondsTolerance = 10;
                    var delta = expectedItem.Created - item.Created;
                    Assert.InRange(delta.TotalSeconds, 0, secondsTolerance);

                    if (expectedItem.Deleted.HasValue == false)
                    {
                        Assert.Equal(expectedItem.Deleted, item.Deleted);
                    }
                    else
                    {
                        Assert.NotNull(expectedItem.Deleted);
                        Assert.NotNull(item.Deleted);
                        var delta2 = item.Deleted - expectedItem.Deleted;
                        Assert.True(delta2.HasValue, "Deleted time delta is null");

                        Assert.InRange(delta2.Value.TotalSeconds, 0, secondsTolerance);
                    }

                }
            }
        }

    }
}
