using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using static AzQueueTestTool.TestCases.Rules.RulesManager;

namespace AzQueueTestTool.TestCases.Rules
{
    class RuleSet11 : IRuleSet
    {
        //serviv
        public CaseId TestCaseId { get => CaseId.TC11; }
        public List<ServicePrincipal> ServicePrincipals { get; set; }

        public bool ValidOwners => false;

        public bool ValidNotes => false;

        public void Execute(List<ServicePrincipal> targetServicePrincipals)
        {
            //-DO NOT set owners 
            //Empty out Notes field

            GraphHelper.ClearNotesField(targetServicePrincipals);

            GraphHelper.ClearOwners(targetServicePrincipals);
        }
    }
}
