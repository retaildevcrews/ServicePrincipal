using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzQueueTestTool.TestCases.ServicePrincipals;
using CSE.Automation.Model;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.Helpers;
using CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using NSubstitute.Exceptions;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates.Discover
{
    internal class DiscoverSpStateDefinition3 : DiscoverSpStateDefinitionBase, IDiscoverSpStateDefinition
    {
        public DiscoverSpStateDefinition3(IConfigurationRoot config, TestCase testCase, GraphDeltaProcessorHelper graphDeltaProcessorHelper) : base(config, testCase, graphDeltaProcessorHelper)
        {
        }
        public override bool Validate()
        {
            if (GraphDeltaProcessorHelper == null)
            {
                throw new Exception($"GraphDeltaProcessorHelper is required for precondition validation for Discover - Test Case [{TestCaseID}]");
            }

            try
            {
                DeleteDynamicCreatedTestServicePrincipals();

                string servicePrincipalAssignedAsNewOwner = $"{DisplayNamePatternFilter}{TestCaseCollection.TestNewUserSuffix}";

                GraphHelper.CreateServicePrincipal(servicePrincipalAssignedAsNewOwner);

                var servicePrincipalList = GraphHelper.GetAllServicePrincipals(servicePrincipalAssignedAsNewOwner).Result;

                if (servicePrincipalList.Count != 1 )
                {
                    throw new InvalidDataException($"Unable Create Sp object as precondition for Discover - Test Case [{TestCaseID}]");
                }
               
                Thread.Sleep(5000);// when running all Test Cases we need to introduce some latency before to run nested FullSeed 
                if (RunFullSeedDiscovery())
                {
                    // we need to add Ownwer to a SP that is part of datalink (current state)
                    servicePrincipalList = AddOwner(servicePrincipalList);

                   // need to introduce some latency before to run the main TestCase #3, after have modified a AAD Test user 
                    Thread.Sleep(5000);

                    TestCaseCollection.ServicePrincipalIdForTestNewUser = servicePrincipalList[0].Id;

                    return servicePrincipalList.Count == 1;
                }
                else
                {
                    throw new InvalidDataException($"Unable to execute RunFullSeedDiscovery as  precondition for Discover - Test Case [{TestCaseID}]");
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to validate precondition for Discover - Test Case [{TestCaseID}]", ex);
            }
            
        }

      

        private List<ServicePrincipal>AddOwner(List<ServicePrincipal> servicePrincipalList)
        {
            Task<List<User>>  users = GraphHelper.GetAllUsers(Config["aadUserServicePrincipalPrefix"]);

            users.Wait();

            List<User> owners = users.Result.Take(1).ToList();

            if (owners.Count == 0)
            {
                new Exception("No AAD users found");
            }

            if (GraphHelper.SetOwners(servicePrincipalList, owners))
            {
                List<ServicePrincipal> result = GraphHelper.GetAllServicePrincipals(servicePrincipalList[0].DisplayName).Result;

                Dictionary<string, string> ownerslist = GraphHelper.GetOwnersDisplayNameAndUserPrincipalNameKeyValuePair(servicePrincipalList[0]);

                if (ownerslist.ContainsKey(owners[0].DisplayName))
                {
                    return result;
                }
                else
                {
                    new Exception("Failed to set Owner");
                }
            }
            else
            {
                new Exception("Unable to set Owner");
            }

            return null;
        }
    }
}
