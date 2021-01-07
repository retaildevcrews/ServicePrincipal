using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using CSE.Automation.Extensions;
using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.KeyVault;
using CSE.Automation.Services;
using Microsoft.Extensions.Configuration;

namespace CSE.Automation.TestsPrep
{
    internal class ConfigurationHelper : IDisposable
    {
        public IConfigurationRoot Config { get;  }

        public GraphHelperSettings GraphSettings { get;  }
      
        public ConfigurationHelper()
        {
            Config = BuildConfiguration();
            GraphSettings = CreateGraphSettings();
        }

        private IConfigurationRoot BuildConfiguration()
        {

            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appconfig.json", true)
                .AddEnvironmentVariables()
                .AddAzureKeyVaultConfiguration(Constants.KeyVaultName);

            return configBuilder.Build();
        }
        private GraphHelperSettings CreateGraphSettings()
        {
            var secretServiceSettings = new SecretServiceSettings() { KeyVaultName = Config[Constants.KeyVaultName] };
            var credServiceSettings = new CredentialServiceSettings() { AuthType = Config[Constants.AuthType].As<AuthenticationType>() };

            ICredentialService credentialService = new CredentialService(credServiceSettings);

            ISecretClient secretClient = new SecretService(secretServiceSettings, credentialService);

            return new GraphHelperSettings(secretClient);

        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
