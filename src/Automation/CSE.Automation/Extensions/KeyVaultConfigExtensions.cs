using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static CSE.Automation.Base.KeyVaultBase;

namespace CSE.Automation.Extensions
{
    public static class KeyVaultConfigExtensions
    {
        public static IConfigurationBuilder AddAzureKeyVaultConfiguration(this IConfigurationBuilder configBuilder, string keyVaultName)
        {
            // build a temporary configuration so we can extract the key vault urls
            if (configBuilder == null) throw new ArgumentNullException(nameof(configBuilder));

            if (!KeyVaultHelper.BuildKeyVaultConnectionString(keyVaultName, out var keyVaultUrlSettingName))
            {
                throw new InvalidDataException("Key vault name not Valid");
            }

            configBuilder.AddAzureKeyVault(keyVaultUrlSettingName);

            return configBuilder;
        }

        public static IConfigurationBuilder AddAzureKeyVaultConfiguration(IConfigurationBuilder configBuilder, Uri keyVaultUrlSettingName)
        {
            throw new NotImplementedException();
        }
    }
}
