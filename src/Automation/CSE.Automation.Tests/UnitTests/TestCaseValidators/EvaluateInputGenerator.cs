using System.Text;
using CSE.Automation.Graph;
using CSE.Automation.Model;
using CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using static CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators
{
    internal class EvaluateInputGenerator : InputGeneratorBase, IInputGenerator
    {

        public string TC4AssignTheseOwnersWhenCreatingAMissingObjectTracking => base._config["TC4_AssignTheseOwnersWhenCreatingAMissingObjectTracking"];


        internal EvaluateInputGenerator(IConfigurationRoot config, GraphHelperSettings graphHelperSettings, ITestCaseCollection testCaseCollection, TestCase testCaseId): base(config, graphHelperSettings,  testCaseCollection, testCaseId)
        {
            
        }
        
        public byte[] GetTestMessageContent(ActivityContext activityContext)
        {
            var myMessage = new QueueMessage<EvaluateServicePrincipalCommand>()
            {
                QueueMessageType = QueueMessageType.Data,
                Document = new EvaluateServicePrincipalCommand
                {
                    CorrelationId = activityContext.CorrelationId, 
                    Model = GetServicePrincipalWrapper().SPModel,
                },
                Attempt = 0
            };

            var payload = JsonConvert.SerializeObject(myMessage);

            var plainTextBytes = Encoding.UTF8.GetBytes(payload);
            return plainTextBytes;
            
        }

    }
}
