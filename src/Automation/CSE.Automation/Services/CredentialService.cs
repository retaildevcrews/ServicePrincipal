// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Azure.Core;
using Azure.Identity;
using CSE.Automation.Interfaces;

namespace CSE.Automation.Services
{
    internal class CredentialService : ICredentialService
    {
        private readonly TokenCredential currentCredential;

        public CredentialService(CredentialServiceSettings settings)
        {
            this.currentCredential = settings.AuthType switch
            {
                AuthenticationType.CLI => new AzureCliCredential(),
                AuthenticationType.MI => new ManagedIdentityCredential(),
                AuthenticationType.VS => new VisualStudioCredential(),
                _ => currentCredential
            };
        }

        public TokenCredential CurrentCredential
        {
            get { return currentCredential; }
        }
    }
}
