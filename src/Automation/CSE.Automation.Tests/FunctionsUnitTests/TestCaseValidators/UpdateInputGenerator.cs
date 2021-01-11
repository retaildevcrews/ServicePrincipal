using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSE.Automation.Graph;
using CSE.Automation.Model;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases;
using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Newtonsoft.Json;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators
{
    internal class UpdateInputGenerator : InputGeneratorBase, IInputGenerator
    {
        public string UTC2AssignTheseOwnersWhenCreatingUpdateQueueMessage => base._config["U_TC2_AssignTheseOwnersWhenCreatingUpdateQueueMessage"];

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
            // NOTE: 'ownersListAsString'  variable represents the correct list of Owners either restored from LKG object or  from ServicePrincipal Owners
            string ownersListAsString = string.Empty;

            // this is the case where Owners are restored from LKG 
            if (TestCaseId == TestCase.TC2 && _validatedServicePrincipalWraper.AADUsers.Count == 0)
            {
                ownersListAsString = string.Join(';', GetAssignedOwnersTestCase2());
            }
            else
            {
                ownersListAsString = string.Join(';', _validatedServicePrincipalWraper.AADUsers);
            }

            // command the AAD Update
            var updateCommand = new ServicePrincipalUpdateCommand()
            {
                CorrelationId = activityContext.CorrelationId,
                ObjectId = _validatedServicePrincipalWraper.AADServicePrincipal.Id,
                Notes = (_validatedServicePrincipalWraper.AADServicePrincipal.Notes, ownersListAsString),
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

        public List<string> GetAssignedOwnersTestCase2()
        {
            if (TestCaseId != TestCase.TC2)
            {
                throw new InvalidOperationException($"This method should only get called for Test case '{TestCase.TC2}].");
            }
            string usersPrefix = AadUserServicePrincipalPrefix;

            var userslList = GraphHelper.GetAllUsers($"{usersPrefix}").Result;

            var toBeAssigned = UTC2AssignTheseOwnersWhenCreatingUpdateQueueMessage.GetAsList();

            List<string> spUsers = new List<string>();
            foreach (var userName in toBeAssigned)
            {
                string userPrincipalName = userslList.FirstOrDefault(x => x.DisplayName == userName.Trim())?.UserPrincipalName;

                if (string.IsNullOrEmpty(userPrincipalName))
                {
                    throw new InvalidDataException($"Unable to get AAD User for assigned Owner user [{userName}].");
                }
                else
                {
                    spUsers.Add(userPrincipalName);
                }
            }

            return spUsers;
        }

    }
}
