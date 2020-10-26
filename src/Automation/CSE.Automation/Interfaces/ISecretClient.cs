using System;
using Azure.Security.KeyVault.Secrets;

namespace CSE.Automation.Interfaces
{
    public interface ISecretClient
    {
        public Uri Uri { get; set; }
        public KeyVaultSecret GetSecret(string secretName);
        public string GetSecretValue(string secretName);
    }
}
