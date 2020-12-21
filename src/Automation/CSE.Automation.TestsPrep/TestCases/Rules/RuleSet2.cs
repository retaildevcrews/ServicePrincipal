using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace CSE.Automation.TestsPrep.TestCases.Rules
{
     internal class RuleSet2 : RuleSetBase, IRuleSet
    {
        public RuleSet2(List<ServicePrincipal> targetServicePrincipals, List<User> targetUsers) : base(targetServicePrincipals, targetUsers)
        {
        }

        public override void Execute()
        {
            base.Execute();

            //-DO NOT set owners 
            //-populated Notes field with owners AAD emails

            HasOwners = GraphHelper.SetOwners(ServicePrincipals, AADUsers);

            GraphHelper.UpdateNotesFieldWithAADOwnersEmail(ServicePrincipals);

            HasOwners = !GraphHelper.ClearOwners(ServicePrincipals);

        }

    }
}
