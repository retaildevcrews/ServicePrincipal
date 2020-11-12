// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Azure.Security.KeyVault.Secrets;
using System;

namespace CSE.Automation.Interfaces
{
    public interface ISecretClient
    {
        public Uri Uri { get; set; }
        public KeyVaultSecret GetSecret(string secretName);
        public string GetSecretValue(string secretName);
    }
}
