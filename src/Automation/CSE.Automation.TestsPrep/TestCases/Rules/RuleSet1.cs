using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace CSE.Automation.TestsPrep.TestCases.Rules
{
    internal class RuleSet1 : RuleSetBase, IRuleSet
    {
        public RuleSet1(List<ServicePrincipal> targetServicePrincipals, List<User> targetUsers) : base (targetServicePrincipals, targetUsers)
        {
        }

        public override void Execute()
        {
            //-set owners 
            //-populated BusinessOwners field with owners AAD emails
            base.Execute();

            HasOwners = GraphHelper.SetOwners(ServicePrincipals, AADUsers);
          
            GraphHelper.UpdateNotesFieldWithAADOwnersEmail(ServicePrincipals);

       }

    }

}
