using System;
using System.Security;
using Azure.Security.KeyVault.Secrets;
namespace CSE.Automation.Interfaces
{
    public interface ISecretClient
    {
        public Uri Uri { get; set; }
        public KeyVaultSecret GetSecret(string secretName);
        public SecureString GetSecretValue(string secretName);
    }
}
