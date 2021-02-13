using System.Text;
using CSE.Automation.Graph;
using CSE.Automation.Model;
using CSE.Automation.Tests.UnitTests.TestCaseValidators.Helpers;
using CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using static CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators
{
    internal class DiscoverInputGenerator : InputGeneratorBase, IInputGenerator
    {


        internal DiscoverInputGenerator(IConfigurationRoot config, GraphHelperSettings graphHelperSettings, ITestCaseCollection testCaseCollection, TestCase testCaseId, string overwriteConfigID = null, GraphDeltaProcessorHelper graphDeltaProcessorHelper = null) 
                                            : base(config, graphHelperSettings, testCaseCollection, testCaseId)
        {
            SetConfigId(overwriteConfigID);
            ValidateDiscoverServicePrincipalPrecondition(testCaseId, graphDeltaProcessorHelper);// the underline logic will throw an exception if fails to validate precondition for the given test case.
        }

        public byte[] GetTestMessageContent(DiscoveryMode discoveryMode, string source, ActivityContext activityContext)
        {
             var myMessage = new QueueMessage<RequestDiscoveryCommand>()
            {
                QueueMessageType = QueueMessageType.Data,
                Document = new RequestDiscoveryCommand
                {
                    CorrelationId = activityContext.CorrelationId,
                    DiscoveryMode = discoveryMode,
                    Source = source,
                },
                Attempt = 0,
            };

            var payload = JsonConvert.SerializeObject(myMessage);

            var plainTextBytes = Encoding.UTF8.GetBytes(payload);
            return plainTextBytes;

        }
    }
}
