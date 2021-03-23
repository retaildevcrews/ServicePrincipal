using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSE.Automation.TestsPrep.TestCases.Rules
{
    internal class RuleSet3 : RuleSetBase, IRuleSet
    {
        public RuleSet3(List<ServicePrincipal> targetServicePrincipals, List<User> targetUsers) : base(targetServicePrincipals, targetUsers)
        {
        }

        public override void Execute()
        {
            base.Execute();
            //-set owners 
            //-populated BusinessOwners field with valid emails other that AAD emails

            HasOwners = GraphHelper.SetOwners(ServicePrincipals, AADUsers);
            
            GraphHelper.UpdateNotesFieldWithValidEmail(ServicePrincipals);

        }

    }
}
