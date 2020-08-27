using Azure.Security.KeyVault.Secrets;
using CSE.Automation.Interfaces;
using CSE.Automation.Utilities;
using System;
using System.Security;

namespace CSE.Automation.KeyVault
{
    public class SecretService : ISecretClient
    {
        private SecretClient secretClient = default;
        private ICredentialService credService = default;

        public SecretService(string keyVaultName, ICredentialService credService)
        {
            //build URI
            string keyVaultUri = default;
            if (!KeyVaultHelper.BuildKeyVaultConnectionString(keyVaultName, out keyVaultUri))
            {
                throw new Exception("Key vault name not Valid"); //TODO: place holder code ensure error message is good and contains input value
            }
            //construct secret client
            secretClient = new SecretClient(new Uri(keyVaultName), credService.CurrentCredential);
        }

        public Uri Uri { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public KeyVaultSecret GetSecret(string secretName)
        {
            return secretClient.GetSecret(secretName);
        }

        public SecureString GetSecretValue(string secretName)
        {
            return SecureStringHelper.ConvertToSecureString(GetSecret(secretName).Value);
        }
    }
}
