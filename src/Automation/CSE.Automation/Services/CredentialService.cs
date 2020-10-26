using Azure.Core;
using Azure.Identity;
using CSE.Automation.Interfaces;

namespace CSE.Automation.Services
{
    public enum AuthenticationType { MI, CLI, VS }

    internal class CredentialServiceSettings
    {
        public AuthenticationType AuthType { get; set; }
    }

    internal class CredentialService : ICredentialService
    {
        private readonly TokenCredential _currentCredential;

        public CredentialService(CredentialServiceSettings settings)
        {
            _currentCredential = settings.AuthType switch
            {
                AuthenticationType.CLI => new AzureCliCredential(),
                AuthenticationType.MI => new ManagedIdentityCredential(),
                AuthenticationType.VS => new VisualStudioCredential(),
                _ => _currentCredential
            };
        }

        public TokenCredential CurrentCredential => _currentCredential;

    }
}
