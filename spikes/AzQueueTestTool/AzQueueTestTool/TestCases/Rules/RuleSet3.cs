using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzQueueTestTool.TestCases.Rules
{
    internal class RuleSet3 : RuleSetBase, IRuleSet
    {
        public RuleSet3(List<ServicePrincipal> targetServicePrincipals) : base(targetServicePrincipals)
        {
        }

        public override void Execute()
        {

            //-set owners 
            //-populated Notes field with valid emails other that AAD emails

            GraphHelper.ClearOwners(ServicePrincipals);

            Task task = GraphHelper.SetOwnersAsync(ServicePrincipals);
            task.Wait();
            GraphHelper.UpdateNotesFieldWithValidEmail(ServicePrincipals);

        }

    }
}
