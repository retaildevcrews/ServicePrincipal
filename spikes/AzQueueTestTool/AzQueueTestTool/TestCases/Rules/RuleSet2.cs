using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace AzQueueTestTool.TestCases.Rules
{
     internal class RuleSet2 : RuleSetBase, IRuleSet
    {
        public RuleSet2(List<ServicePrincipal> targetServicePrincipals) : base(targetServicePrincipals)
        {
        }

        public override void Execute()
        {

            //-DO NOT set owners 
            //-populated Notes field with owners AAD emails

            GraphHelper.ClearOwners(ServicePrincipals);

            Task task = GraphHelper.SetOwnersAsync(ServicePrincipals);

            task.Wait();
            GraphHelper.UpdateNotesFieldWithAADOwnersEmail(ServicePrincipals);

            GraphHelper.ClearOwners(ServicePrincipals);

        }

    }
}
