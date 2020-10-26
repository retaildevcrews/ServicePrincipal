using Azure.Core;

namespace CSE.Automation.Interfaces
{
    public interface ICredentialService
    {
        public TokenCredential CurrentCredential { get; }
    }
}
