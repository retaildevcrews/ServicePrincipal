using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using Azure.Core;
using CSE.Automation.Interfaces;
using Azure.Identity;

namespace CSE.Automation.Services
{
    public enum AuthenticationType { MI, CLI, VS }
    class CredentialService : ICredentialService
    {
        TokenCredential currentCredential = default;

        public CredentialService (string credType)
        {
            if (credType == "CLI")
            {
                currentCredential = new AzureCliCredential();
            }
            else if(credType == "MI")
            {
                currentCredential = new ManagedIdentityCredential();
            }
            else if(credType == "VS")
            {
                currentCredential = new VisualStudioCredential();
            }
        }

        public TokenCredential CurrentCredential
        { 
            get { return currentCredential; }
        }
        
    }
}
