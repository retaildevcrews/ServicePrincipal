using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace AzQueueTestTool.TestCases.Rules
{
     internal class RuleSet6 : RuleSetBase, IRuleSet
    {
        public RuleSet6(List<ServicePrincipal> targetServicePrincipals) : base(targetServicePrincipals)
        {
        }

        public override void Execute()
        {

            // set owners 
            //Empty out Notes field

            GraphHelper.ClearOwners(ServicePrincipals);

            Task task = GraphHelper.SetOwnersAsync(ServicePrincipals);
            task.Wait();
            GraphHelper.ClearNotesField(ServicePrincipals);

        }

    }
}
