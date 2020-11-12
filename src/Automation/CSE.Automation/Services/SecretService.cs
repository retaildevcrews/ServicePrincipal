// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Azure.Security.KeyVault.Secrets;
using CSE.Automation.Base;
using CSE.Automation.Extensions;
using CSE.Automation.Interfaces;
using System;
using System.Configuration;

namespace CSE.Automation.KeyVault
{
    public class SecretService : KeyVaultBase, ISecretClient
    {
        private readonly SecretClient secretClient;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Instances are injected via DI and are guaranteed to be non-null")]
        public SecretService(SecretServiceSettings settings, ICredentialService credService)
        {
            // build URI
            if (!KeyVaultHelper.BuildKeyVaultConnectionString(settings.KeyVaultName, out var keyVaultUri))
            {
                throw new Exception("Key vault name not Valid"); // TODO: place holder code ensure error message is good and contains input value
            }

            Uri = new Uri(keyVaultUri);

            // construct secret client
            secretClient = new SecretClient(Uri, credService.CurrentCredential);
        }

        public Uri Uri { get; set; }

        public KeyVaultSecret GetSecret(string secretName)
        {
            return secretClient.GetSecret(secretName);
        }

        public string GetSecretValue(string secretName)
        {
            return GetSecret(secretName).Value;
        }
    }
}
