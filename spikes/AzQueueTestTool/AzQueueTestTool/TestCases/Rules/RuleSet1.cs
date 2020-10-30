using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace AzQueueTestTool.TestCases.Rules
{
    internal class RuleSet1 : RuleSetBase, IRuleSet
    {
        public RuleSet1(List<ServicePrincipal> targetServicePrincipals, List<User> targetUsers) : base (targetServicePrincipals, targetUsers)
        {
        }

        public override void Execute()
        {
            //-set owners 
            //-populated Notes field with owners AAD emails
            base.Execute();

            HasOwners = GraphHelper.SetOwners(ServicePrincipals, AADUsers);
          
            GraphHelper.UpdateNotesFieldWithAADOwnersEmail(ServicePrincipals);

       }

    }

}
