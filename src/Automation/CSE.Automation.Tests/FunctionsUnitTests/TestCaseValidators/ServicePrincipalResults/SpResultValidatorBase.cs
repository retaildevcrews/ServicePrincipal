using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzQueueTestTool.TestCases.ServicePrincipals;
using CSE.Automation.Model;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Graph;
using Newtonsoft.Json;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalResults
{

    abstract class SpResultValidatorBase : ISpResultValidator
    {
        public string SavedServicePrincipalAsString { get; }

        public ServicePrincipal NewServicePrincipal { get; }

        public TestCase TestCaseID { get; }

        private readonly InputGenerator _inputGenerator;

        private readonly ActivityContext _activityContext;


        public SpResultValidatorBase(string savedServicePrincipalAsString, InputGenerator inputGenerator, ActivityContext activityContext)
        {
            SavedServicePrincipalAsString = savedServicePrincipalAsString;

            _inputGenerator = inputGenerator;
            _activityContext = activityContext;

            NewServicePrincipal = _inputGenerator.GetServicePrincipal(true);
            TestCaseID = _inputGenerator.TestCaseId;
        }

        public abstract bool Validate();


        public bool DoesUpdateMessageExistInQueue()
        {
            var storageAccount = CloudStorageAccount.Parse(_inputGenerator.StorageConnectionString);
            var cmdQueue = storageAccount.CreateCloudQueueClient().GetQueueReference(_inputGenerator.UpdateQueueName);

            Dictionary<string,string> ownersInfoList = GraphHelper.GetOwnersDisplayNameAndUserPrincipalNameKeyValuePair(NewServicePrincipal);
            List<string> ownersList = ownersInfoList.Values.ToList();

            List<string> spCurrentNotesList = NewServicePrincipal.Notes.Split(";").ToList();

            object foundLock = new object();
            bool messageFound = false;
            while (!messageFound)
            {
                //TODO Can I turn this into Parallel loop to proceess multiple  batches of 32  Parallel.ForEach

                IEnumerable <CloudQueueMessage> cmdMessages = cmdQueue.GetMessages(32);// 32 is the max number of messages that can be retrived 

                if (cmdMessages.Count() == 0)
                {
                    break;
                }

                Parallel.ForEach(cmdMessages, (msg, state) =>
                {
                    if (msg != null && !string.IsNullOrEmpty(msg.AsString))
                    {
                        var command = JsonConvert.DeserializeObject<QueueMessage<ServicePrincipalUpdateCommand>>(msg.AsString).Document;

                        var changeOwners = command.Notes.Changed.Split(";").ToList();
                        var currentNotes = command.Notes.Current.Split(";").ToList();
                       
                        lock (foundLock)// needed for Parallel foreach only
                        {
                            messageFound = command.CorrelationId == _activityContext.CorrelationId && command.Id == NewServicePrincipal.Id
                                     && spCurrentNotesList.Except(currentNotes).Count() == 0 // makes sure both list contain same items 
                                     && ownersList.Except(changeOwners).Count() == 0 // makes sure both list contain same items 
                                     && command.Message.Equals("Update Notes from Owners", StringComparison.InvariantCultureIgnoreCase); // we need to use Enums instead of "Strings"

                            if (messageFound)
                            {
                                state.Break();  
                            }
                        }

                    }
                  });
            }

            return messageFound;
        }
    }
}
