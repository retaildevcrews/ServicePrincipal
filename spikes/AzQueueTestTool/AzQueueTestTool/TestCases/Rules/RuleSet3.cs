using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using static AzQueueTestTool.TestCases.Rules.RulesManager;

namespace AzQueueTestTool.TestCases.Rules
{
    class RuleSet3 : IRuleSet
    {
        //serviv
        public CaseId TestCaseId { get => CaseId.TC3; }
        public List<ServicePrincipal> ServicePrincipals { get; set; }

        public bool ValidOwners => true;

        public bool ValidNotes => false;

        public void CreateServicePrincipals()
        {
            //Create X service principals or re-uses existing ones matching the SP pattern 
            //-set owners 
            //-populated Notes field with valid emails other tht AAD emails
        }
    }
}
