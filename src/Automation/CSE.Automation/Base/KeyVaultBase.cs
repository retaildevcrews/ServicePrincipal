using System;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation.Base
{
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable
    public class KeyVaultBase
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
#pragma warning disable CA1034 // Nested types should not be visible
        public static class KeyVaultHelper
#pragma warning restore CA1034 // Nested types should not be visible
        {

            /// <summary>
            /// Build the Key Vault URL from the name
            /// </summary>
            /// <param name="name">Key Vault Name</param>
            /// <returns>URL to Key Vault</returns>
            public static bool BuildKeyVaultConnectionString(string keyVaultName, out string keyvaultConnection)
            {
                // name is required
                if (string.IsNullOrWhiteSpace(keyVaultName))
                {
                    throw new ArgumentNullException(nameof(keyVaultName));
                }

                var uriBuilder = new UriBuilder
                {
                    Scheme = Uri.UriSchemeHttps,
                    Host = $"{keyVaultName}.vault.azure.net"
                };

                keyvaultConnection = uriBuilder.Uri.AbsoluteUri;

                return true;
            }

            /// <summary>
            /// Validate the keyvault name
            /// </summary>
            /// <param name="name">string</param>
            /// <returns>bool</returns>
            public static bool ValidateName(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return false;
                }
                name = name.Trim();

                return name.Length >= 3 && name.Length <= 24;
            }
        }
    }
}
