using System.Text;
using AzQueueTestTool.TestCases.ServicePrincipals;
using CSE.Automation.Graph;
using CSE.Automation.Model;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Newtonsoft.Json;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators
{
    internal class UpdateInputGenerator : InputGeneratorBase, IInputGenerator
    {

        private ServicePrincipalWrapper _validatedServicePrincipalWraper;

        internal UpdateInputGenerator(IConfigurationRoot config, GraphHelperSettings graphHelperSettings, ITestCaseCollection testCaseCollection, TestCase testCaseId): base(config, graphHelperSettings,  testCaseCollection, testCaseId)
        {
            _validatedServicePrincipalWraper = ValidateUpdateServicePrincipalPrecondition(testCaseId);// the underline logic will throw an exception if fails to validate precondition for the given test case.
        }

        public override ServicePrincipal GetServicePrincipal(bool requery = false)
        {
            if (requery)
            {
                return GraphHelper.GetServicePrincipal(_validatedServicePrincipalWraper.AADServicePrincipal.DisplayName).Result;
            }
            else
            {
                return _validatedServicePrincipalWraper.AADServicePrincipal;
            }
        }

        public override  ServicePrincipalModel GetServicePrincipalModel()
        {
            return _validatedServicePrincipalWraper.SPModel;
        }

        public byte[] GetTestMessageContent(ActivityContext activityContext)
        {

            if (TestCaseId == TestCase.TC2)
            {
                //TODO
                string assignedOwners = _config["UTC4_AssignTheseOwnersWhen....."];
            }

            var ownersList = string.Join(';', _validatedServicePrincipalWraper.AADUsers);

            // command the AAD Update
            var updateCommand = new ServicePrincipalUpdateCommand()
            {
                CorrelationId = activityContext.CorrelationId,
                Id = _validatedServicePrincipalWraper.AADServicePrincipal.Id,
                Notes = (_validatedServicePrincipalWraper.AADServicePrincipal.Notes, ownersList),
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
