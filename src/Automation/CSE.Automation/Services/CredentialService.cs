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

        public CredentialService (AuthenticationType credType)
        {
            if (credType == AuthenticationType.CLI)
            {
                currentCredential = new AzureCliCredential();
            }
            else if(credType == AuthenticationType.MI)
            {
                currentCredential = new ManagedIdentityCredential();
            }
            else if(credType == AuthenticationType.VS)
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
