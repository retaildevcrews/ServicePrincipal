// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Azure.Core;
using Azure.Identity;
using CSE.Automation.Interfaces;

namespace CSE.Automation.Services
{
    internal enum AuthenticationType
    {
        MI,
        CLI,
        VS,
    }

    internal class CredentialServiceSettings
    {
        public AuthenticationType AuthType { get; set; }
    }
}
