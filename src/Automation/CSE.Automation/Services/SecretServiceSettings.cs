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
    public class SecretServiceSettings : ISettingsValidator
    {
        public string KeyVaultName { get; set; }
        public void Validate()
        {
            if (this.KeyVaultName.IsNull())
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: KeyVaultName is invalid");
            }
        }
    }
}
