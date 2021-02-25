// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using CSE.Automation.Extensions;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using SettingsBase = CSE.Automation.Model.SettingsBase;

namespace CSE.Automation.Graph
{
    internal class GraphHelperSettings : SettingsBase, IGraphHelperSettings
    {
        public GraphHelperSettings(ISecretClient secretClient)
                : base(secretClient)
        {
        }

        [Secret(Constants.GraphAppClientIdKey)]
        public string GraphAppClientId => GetSecret();

        [Secret(Constants.GraphAppTenantIdKey)]
        public string GraphAppTenantId => GetSecret();

        [Secret(Constants.GraphAppClientSecretKey)]
        public string GraphAppClientSecret => GetSecret();

        public override void Validate()
        {
            if (this.GraphAppClientId.IsNull())
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: GraphAppClientId is null");
            }

            if (this.GraphAppTenantId.IsNull())
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: GraphAppTenantId is null");
            }

            if (this.GraphAppClientSecret.IsNull())
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: GraphAppClientSecret is null");
            }
        }
    }
}
