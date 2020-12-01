using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzQueueTestTool.TestCases.ServicePrincipals;
using CSE.Automation.Model;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.System.ComponentModel;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Xunit.Sdk;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators
{
    internal class DiscoverInputGenerator : InputGeneratorBase, IInputGenerator
    {
      
        internal DiscoverInputGenerator(IConfigurationRoot config, ActivityContext activityContext, ITestCaseCollection testCaseCollection, TestCase testCaseId) : base(config, activityContext, testCaseCollection, testCaseId)
        {

        }

        public byte[] GetTestMessageContent(DiscoveryMode discoveryMode, string source)
        {
             var myMessage = new QueueMessage<RequestDiscoveryCommand>()
            {
                QueueMessageType = QueueMessageType.Data,
                Document = new RequestDiscoveryCommand
                {
                    CorrelationId = _activityContext.CorrelationId,
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
