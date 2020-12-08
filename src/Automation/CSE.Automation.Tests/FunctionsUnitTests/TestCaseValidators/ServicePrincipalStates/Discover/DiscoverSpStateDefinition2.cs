using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalStates.Discover
{
    internal class DiscoverSpStateDefinition2 : DiscoverSpStateDefinitionBase, IDiscoverSpStateDefinition
    {
        public DiscoverSpStateDefinition2( string displayNamePatternFilter, TestCase testCase) : base(displayNamePatternFilter, testCase)
        {
        }
        public override bool Validate()
        {
            string servicePrincipalToDelete = $"{DisplayNamePatternFilter}-REMOVED";

            var servicePrincipalList = GraphHelper.GetAllServicePrincipals(servicePrincipalToDelete).Result;

            if (servicePrincipalList.Count == 0)
            {
                GraphHelper.CreateServiceThisPrincipal(servicePrincipalToDelete);

                servicePrincipalList = GraphHelper.GetAllServicePrincipals(servicePrincipalToDelete).Result;

            }

            //TODO: add  @Remove atttribute
            //  attributeName:"AdditionalData",
            //existingAttributeValue: "@removed"));

            servicePrincipalList[0].AdditionalData.Add("@removed", "injected removed attribute");

            Task updateTask = Task.Run( () =>  GraphHelper.UpdateGraphObject(servicePrincipalList[0]));

            updateTask.Wait();

            return false;

        }
    }
}
