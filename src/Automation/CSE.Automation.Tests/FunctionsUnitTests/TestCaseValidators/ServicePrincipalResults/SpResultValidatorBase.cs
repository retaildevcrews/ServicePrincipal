using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSE.Automation.Model;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.Helpers;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Graph;
using Newtonsoft.Json;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalResults
{

    abstract class SpResultValidatorBase : ISpResultValidator
    {
        public string SavedServicePrincipalAsString { get; }

        public ServicePrincipal NewServicePrincipal { get; }

        public TestCase TestCaseID { get; }

        private readonly IInputGenerator _inputGenerator;

        private readonly ActivityContext _activityContext;

        protected string DisplayNamePatternFilter 
        { 
            get 
            { 
                return _inputGenerator.DisplayNamePatternFilter; 
            } 
        }


        protected IInputGenerator InputGeneratorInstance
        {
            get
            {
                return _inputGenerator;
            }
        }

        public SpResultValidatorBase(string savedServicePrincipalAsString, IInputGenerator inputGenerator, ActivityContext activityContext, bool getNewServicePrincipal = true)
        {
            SavedServicePrincipalAsString = savedServicePrincipalAsString;

            _inputGenerator = inputGenerator;
            _activityContext = activityContext;

            TestCaseID = _inputGenerator.TestCaseId;

            if (getNewServicePrincipal)
            {
                NewServicePrincipal = _inputGenerator.GetServicePrincipal(true);
            }
        }

        public abstract bool Validate();


        public bool DoesMessageExistInUpdateQueue(List<ServicePrincipalUpdateAction> targetQueueMessages)
        {
            var storageAccount = CloudStorageAccount.Parse(_inputGenerator.StorageConnectionString);
            var cmdQueue = storageAccount.CreateCloudQueueClient().GetQueueReference(_inputGenerator.UpdateQueueName);

            object foundLock = new object();
            bool messageFound = false;
            while (!messageFound)
            {

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
                      
                        lock (foundLock)// needed for Parallel foreach only
                        {
                            if (!messageFound)/// state.Break(); takes its time and does not break the loop immediately 
                            {
                                messageFound = command.CorrelationId == _activityContext.CorrelationId && command.ObjectId == NewServicePrincipal.Id
                                              && targetQueueMessages.Contains(command.Action); // we need to use Enums instead of "Strings"


                                if (messageFound)
                                {
                                    state.Break();
                                }
                            }
                        }

                    }
                  });

                if (messageFound)
                    break; // break While loop
            }

            return messageFound;
        }


        public int GetMessageCountInEvaluateQueueFor(string displayNamePatternFilter)
        {
            var storageAccount = CloudStorageAccount.Parse(_inputGenerator.StorageConnectionString);
            var cmdQueue = storageAccount.CreateCloudQueueClient().GetQueueReference(_inputGenerator.EvaluateQueueName);


            int result = 0;
            object foundLock = new object();

            IEnumerable <CloudQueueMessage> cmdMessages = cmdQueue.GetMessages(32);// 32 is the max number of messages that can be retrived 

            while (cmdMessages.Count() > 0)
            {

                Parallel.ForEach(cmdMessages, (msg, state) =>
                {
                    if (msg != null && !string.IsNullOrEmpty(msg.AsString))
                    {
                        var command = JsonConvert.DeserializeObject<QueueMessage<EvaluateServicePrincipalCommand>>(msg.AsString).Document;

                        lock (foundLock)// needed for Parallel foreach only
                        {
                            bool messageFound = command.CorrelationId == _activityContext.CorrelationId
                                          && command.Model.DisplayName.StartsWith(displayNamePatternFilter);

                            if (messageFound)
                            {
                                result++;
                            }

                        }
                    }
                });

                cmdMessages = cmdQueue.GetMessages(32);
            }

            return result;
        }

        public bool DoesMessageExistInEvaluateQueue(string servicePrincipalId)
        {
            var storageAccount = CloudStorageAccount.Parse(_inputGenerator.StorageConnectionString);
            var cmdQueue = storageAccount.CreateCloudQueueClient().GetQueueReference(_inputGenerator.EvaluateQueueName);

            object foundLock = new object();
            bool messageFound = false;
            while (!messageFound)
            {

                IEnumerable <CloudQueueMessage> cmdMessages = cmdQueue.GetMessages(32);// 32 is the max number of messages that can be retrived 

                if (cmdMessages.Count() == 0)
                {
                    break;
                }

                Parallel.ForEach(cmdMessages, (msg, state) =>
                {
                    if (msg != null && !string.IsNullOrEmpty(msg.AsString))
                    {
                        var command = JsonConvert.DeserializeObject<QueueMessage<EvaluateServicePrincipalCommand>>(msg.AsString).Document;

                        lock (foundLock)// needed for Parallel foreach only
                        {
                            if (!messageFound)/// state.Break(); takes its time and does not break the loop immediately 
                            {
                                messageFound = command.CorrelationId == _activityContext.CorrelationId 
                                              && servicePrincipalId == command.Model.Id; 


                                if (messageFound)
                                {
                                    state.Break();
                                }
                            }
                        }

                    }
                });

                if (messageFound)
                    break; // break While loop
            }

            return messageFound;
        }

        internal void DeleteServicePrincipal(string servicePrincipalToDelete)
        {
            using ServicePrincipalHelper servicePrincipalHelper = new ServicePrincipalHelper();
            servicePrincipalHelper.DeleteServicePrincipal(servicePrincipalToDelete);
        }
    }
}
