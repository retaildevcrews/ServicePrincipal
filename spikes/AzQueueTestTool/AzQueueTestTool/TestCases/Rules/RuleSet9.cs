using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using static AzQueueTestTool.TestCases.Rules.RulesManager;

namespace AzQueueTestTool.TestCases.Rules
{
    class RuleSet9 : IRuleSet
    {
        //serviv
        public CaseId TestCaseId { get => CaseId.TC9; } //CaseId.TC6; 
        public List<ServicePrincipal> ServicePrincipals { get; set; }

        public bool ValidOwners => true;

        public bool ValidNotes => false;

        public void Execute(List<ServicePrincipal> targetServicePrincipals)
        {
            //-set owners 
            //Empty out Notes field

            GraphHelper.SetOwners(targetServicePrincipals);

            GraphHelper.ClearNotesFiled(targetServicePrincipals);


        }
    }
}
