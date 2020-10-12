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

    class CredentialServiceSettings
    {
        public AuthenticationType AuthType { get; set; }
    }
    class CredentialService : ICredentialService
    {
        readonly TokenCredential _currentCredential;

        public CredentialService (CredentialServiceSettings settings)
        {
            _currentCredential = settings.AuthType switch
            {
                AuthenticationType.CLI => new AzureCliCredential(),
                AuthenticationType.MI => new ManagedIdentityCredential(),
                AuthenticationType.VS => new VisualStudioCredential(),
                _ => _currentCredential
            };
        }

        public TokenCredential CurrentCredential
        { 
            get { return _currentCredential; }
        }
        
    }
}
