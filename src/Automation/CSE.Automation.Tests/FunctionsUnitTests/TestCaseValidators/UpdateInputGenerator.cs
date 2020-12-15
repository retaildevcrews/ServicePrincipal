using System.Text;
using CSE.Automation.Graph;
using CSE.Automation.Model;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators
{
    internal class UpdateInputGenerator : InputGeneratorBase, IInputGenerator
    {

        public string TC4AssignTheseOwnersWhenCreatingAMissingObjectTracking => base._config["TC4_AssignTheseOwnersWhenCreatingAMissingObjectTracking"];


        internal UpdateInputGenerator(IConfigurationRoot config, GraphHelperSettings graphHelperSettings, ITestCaseCollection testCaseCollection, TestCase testCaseId): base(config, graphHelperSettings,  testCaseCollection, testCaseId)
        {
            
        }
        
        public byte[] GetTestMessageContent(ActivityContext activityContext)
        {
            
            var servicePrincipal = GetServicePrincipalForUpdateQueue();

            // TODO: <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            //need to set Owners to servicePrincipal
            var ownersList = string.Join(';', servicePrincipal.Owners);

            // command the AAD Update
            var updateCommand = new ServicePrincipalUpdateCommand()
            {
                CorrelationId = activityContext.CorrelationId,
                Id = servicePrincipal.Id,
                Notes = (servicePrincipal.Notes, ownersList),
                Action = ServicePrincipalUpdateAction.Update, // "Update Notes from Owners",
            };

            var myMessage = new QueueMessage<ServicePrincipalUpdateCommand>()
            {
                QueueMessageType = QueueMessageType.Data,

                Document = updateCommand,
                Attempt = 0
            };

            var payload = JsonConvert.SerializeObject(myMessage);

            var plainTextBytes = Encoding.UTF8.GetBytes(payload);
            return plainTextBytes;
            
        }

    }
}
