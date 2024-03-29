﻿using System.Text;
using CSE.Automation.Graph;
using CSE.Automation.Model;
using CSE.Automation.Model.Commands;
using CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using static CSE.Automation.Tests.IntegrationTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators
{
    internal class EvaluateInputGenerator : InputGeneratorBase, IInputGenerator
    {

        public string TC4AssignTheseOwnersWhenCreatingAMissingObjectTracking => base._config["TC4_AssignTheseOwnersWhenCreatingAMissingObjectTracking"];


        internal EvaluateInputGenerator(IConfigurationRoot config, GraphHelperSettings graphHelperSettings, ITestCaseCollection testCaseCollection, TestCase testCaseId): base(config, graphHelperSettings,  testCaseCollection, testCaseId)
        {
            
        }
        
        public byte[] GetTestMessageContent(ActivityContext activityContext)
        {
            var myMessage = new QueueMessage<ServicePrincipalEvaluateCommand>()
            {
                QueueMessageType = QueueMessageType.Data,
                Document = new ServicePrincipalEvaluateCommand
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
