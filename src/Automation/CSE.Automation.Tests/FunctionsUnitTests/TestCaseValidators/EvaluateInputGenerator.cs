using System.Text;
using CSE.Automation.Model;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators
{
    internal class EvaluateInputGenerator : InputGeneratorBase, IInputGenerator
    {

        public string TC4AssignTheseOwnersWhenCreatingAMissingObjectTracking => base._config["TC4_AssignTheseOwnersWhenCreatingAMissingObjectTracking"];


        internal EvaluateInputGenerator(IConfigurationRoot config, ITestCaseCollection testCaseCollection, TestCase testCaseId): base(config,  testCaseCollection, testCaseId)
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
