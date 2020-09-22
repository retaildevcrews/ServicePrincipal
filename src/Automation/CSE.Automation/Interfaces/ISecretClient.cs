using Azure.Security.KeyVault.Secrets;
using System;

namespace CSE.Automation.Interfaces
{
    public interface ISecretClient
    {
        public Uri Uri { get; set; }
        public KeyVaultSecret GetSecret(string secretName);
        public string GetSecretValue(string secretName);
    }
}
