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
            private static string _keyVaultConnectionString;

            /// <summary>
            /// Build the Key Vault URL from the name
            /// </summary>
            /// <param name="name">Key Vault Name</param>
            /// <returns>URL to Key Vault</returns>
            public static bool BuildKeyVaultConnectionString(out string keyvaultConnection)
            {
                keyvaultConnection = Environment.GetEnvironmentVariable(Constants.KeyVaultName);

                keyvaultConnection = keyvaultConnection?.Trim();

                // name is required
                if (string.IsNullOrWhiteSpace(keyvaultConnection))
                {
                    return false;
                }


                if (string.IsNullOrWhiteSpace(_keyVaultConnectionString))
                {

                    // build the URL
                    if (!keyvaultConnection.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        keyvaultConnection = "https://" + keyvaultConnection;
                    }

                    if (!keyvaultConnection.EndsWith(".vault.azure.net/", StringComparison.OrdinalIgnoreCase) && !keyvaultConnection.EndsWith(".vault.azure.net", StringComparison.OrdinalIgnoreCase))
                    {
                        keyvaultConnection += ".vault.azure.net/";
                    }

                    if (!keyvaultConnection.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        keyvaultConnection += "/";
                    }
                    _keyVaultConnectionString = keyvaultConnection;
                }
                else
                {
                    keyvaultConnection = _keyVaultConnectionString;
                }

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

                if (name.Length < 3 || name.Length > 24)
                {
                    return false;
                }

                return true;
            }
        }
    }
}
