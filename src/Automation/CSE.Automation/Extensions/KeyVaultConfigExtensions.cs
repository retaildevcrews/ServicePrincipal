using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;

namespace CSE.Automation.Extensions
{
    // This code taken from: https://github.com/briandunnington/AzureFunctionsKeyVaultSettingsExample/blob/master/AzureFunctionsKeyVaultSettingsExample/KeyVaultConfigExtensions.cs
    // Thank you Brain Dunnington!
    public static class KeyVaultConfigExtensions
    {
        public static IConfiguration AddAzureKeyVaultConfiguration(this IWebJobsBuilder builder, string keyVaultUrlSettingName)
        {
            return builder.Services.AddAzureKeyVaultConfiguration(keyVaultUrlSettingName);
        }

        public static IConfiguration AddAzureKeyVaultConfiguration(this IFunctionsHostBuilder builder, string keyVaultUrlSettingName)
        {
            return builder.Services.AddAzureKeyVaultConfiguration(keyVaultUrlSettingName);
        }

        /* This is the meat of the logic. It finds the IConfiguration service that is already registered by the runtime, and then
         * creates an instance of the concrete class if necessary. It loads all of the Key Vault secrets into the config, and then
         * replaces the registered IConfiguration instance with the patched version.
         * 
         * Use at your own risk 😊
         * */
        public static IConfiguration AddAzureKeyVaultConfiguration(this IServiceCollection services, string keyVaultUrlSettingName)
        {
            // get the IConfiguration that is already registered with the host
            var configBuilder = new ConfigurationBuilder();
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IConfiguration));
            if (descriptor?.ImplementationInstance is IConfigurationRoot configuration)
            {
                configBuilder.AddConfiguration(configuration);
            }
            else if (descriptor?.ImplementationFactory != null)
            {
                var sp = services.BuildServiceProvider();
                var constructedConfiguration = descriptor.ImplementationFactory.Invoke(sp) as IConfiguration;
                configBuilder.AddConfiguration(constructedConfiguration);
            }

            // build a temporary configuration so we can extract the key vault urls
            var tempConfig = configBuilder.Build();
            var keyVaultUrls = tempConfig[keyVaultUrlSettingName]?.Split(',');

            // add the key vault providers and build a new config
            configBuilder.AddAzureKeyVaults(keyVaultUrls);
            var config = configBuilder.Build();

            // replace the existing IConfiguration instance with our new one
            services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

            return config;
        }

        public static IConfigurationBuilder AddAzureKeyVaults(this IConfigurationBuilder builder, params string[] keyVaultEndpoints)
        {
            if (builder != null && keyVaultEndpoints != null)
            {
                for (int i = 0; i < keyVaultEndpoints.Length; i++)
                {
                    var keyVaultEndpoint = keyVaultEndpoints[i];
                    if (!string.IsNullOrWhiteSpace(keyVaultEndpoint))
                        builder.AddAzureKeyVault(keyVaultEndpoints[i]);
                }
            }
            return builder;
        }
    }
}
