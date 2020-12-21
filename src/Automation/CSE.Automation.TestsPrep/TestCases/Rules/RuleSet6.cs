using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace CSE.Automation.TestsPrep.TestCases.Rules
{
     internal class RuleSet6 : RuleSetBase, IRuleSet
    {
        public RuleSet6(List<ServicePrincipal> targetServicePrincipals, List<User> targetUsers) : base(targetServicePrincipals, targetUsers)
        {
        }

        public override void Execute()
        {
            base.Execute();
            // set owners 
            //Empty out Notes field

            HasOwners = GraphHelper.SetOwners(ServicePrincipals, AADUsers);
            
            GraphHelper.ClearNotesField(ServicePrincipals);

        }

    }
}
