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


        internal EvaluateInputGenerator(IConfigurationRoot config, ActivityContext activityContext, ITestCaseCollection testCaseCollection, TestCase testCaseId): base(config,  activityContext, testCaseCollection, testCaseId)
        {
            
        }
        
        public byte[] GetTestMessageContent()
        {
            var myMessage = new QueueMessage<EvaluateServicePrincipalCommand>()
            {
                QueueMessageType = QueueMessageType.Data,
                Document = new EvaluateServicePrincipalCommand
                {
                    CorrelationId = _activityContext.CorrelationId, 
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
