using System;
using System.Security;
using Azure.Security.KeyVault.Secrets;
using CSE.Automation.Interfaces;

namespace CSE.Automation.KeyVault 
{
    public class SecretService : ISecretClient
    {
        private SecretClient secretClient = default;
        public SecretService(string keyVaultName)
        {
        }

        public Uri Uri { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public KeyVaultSecret GetSecret(string secretName)
        {
            throw new NotImplementedException();
        }

        public SecureString GetSecretValue(string secretName)
        {
            throw new NotImplementedException();
        }
    }
}
