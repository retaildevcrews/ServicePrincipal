using Azure.Security.KeyVault.Secrets;
using CSE.Automation.Base;
using CSE.Automation.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using CSE.Automation.Extensions;

namespace CSE.Automation.KeyVault
{
    public class SecretServiceSettings : ISettingsValidator
    {
        public string KeyVaultName { get; set; }
        public void Validate()
        {
            if (this.KeyVaultName.IsNull()) throw new ConfigurationErrorsException($"{this.GetType().Name}: KeyVaultName is invalid");
        }
    }

    public class SecretService : KeyVaultBase, ISecretClient
    {
        private readonly SecretClient _secretClient;

        public SecretService(SecretServiceSettings settings, ICredentialService credService)
        {
            //build URI
            if (!KeyVaultHelper.BuildKeyVaultConnectionString(out var keyVaultUri))
            {
                throw new Exception("Key vault name not Valid"); //TODO: place holder code ensure error message is good and contains input value
            }
            Uri = new Uri(keyVaultUri);

            //construct secret client
            _secretClient = new SecretClient(Uri, credService.CurrentCredential);
        }

        public Uri Uri { get; set; }

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
