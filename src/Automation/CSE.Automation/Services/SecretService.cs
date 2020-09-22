using Azure.Security.KeyVault.Secrets;
using CSE.Automation.Base;
using CSE.Automation.Interfaces;
using System;

namespace CSE.Automation.KeyVault
{
    public class SecretService : KeyVaultBase, ISecretClient
    {
        private SecretClient _secretClient;
        private ICredentialService _credService;

        public SecretService(string keyVaultName, ICredentialService credService)
        {
            _credService = credService;
            //build URI
            string keyVaultUri = default;
            if (!KeyVaultHelper.BuildKeyVaultConnectionString(keyVaultName, out keyVaultUri))
            {
                throw new Exception("Key vault name not Valid"); //TODO: place holder code ensure error message is good and contains input value
            }
            Uri = new Uri(keyVaultUri);

            //construct secret client
            if (_credService != null)
                _secretClient = new SecretClient(Uri, _credService.CurrentCredential);
            else
                throw new Exception("Credential Service is Null in SecretService");
        }

        public Uri Uri { get; set;}

        public KeyVaultSecret GetSecret(string secretName)
        {
            return _secretClient.GetSecret(secretName);
        }

        public string GetSecretValue(string secretName)
        {
            return GetSecret(secretName).Value;
        }
    }
}
