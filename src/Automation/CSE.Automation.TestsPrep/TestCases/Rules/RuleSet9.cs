using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSE.Automation.TestsPrep.TestCases.Rules
{
    internal class RuleSet9 : RuleSetBase, IRuleSet
    {
        public RuleSet9(List<ServicePrincipal> targetServicePrincipals, List<User> targetUsers) : base(targetServicePrincipals, targetUsers)
        {

        }

        public override void Execute()
        {
            base.Execute();
            // set owners 
            //Empty out BusinessOwners field

            HasOwners = GraphHelper.SetOwners(ServicePrincipals, AADUsers);
            
            GraphHelper.ClearNotesField(ServicePrincipals);

        }

    }
}
