using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using static AzQueueTestTool.TestCases.Rules.RulesManager;

namespace AzQueueTestTool.TestCases.Rules
{
    class RuleSet4 : IRuleSet
    {
        //serviv
        public CaseId TestCaseId { get => CaseId.TC4; }// CaseId.TC5;
        public List<ServicePrincipal> ServicePrincipals { get; set; }

        public bool ValidOwners => false;

        public bool ValidNotes => false;

        public void Execute(List<ServicePrincipal> targetServicePrincipals)
        {

            //-DO NOT set owners 
            //-populated Notes field with valid emails other that AAD emails

            
            GraphHelper.ClearOwners(targetServicePrincipals);

            GraphHelper.UpdateNotesFieldWithValidEmail(targetServicePrincipals);
            

        }
    }
}
