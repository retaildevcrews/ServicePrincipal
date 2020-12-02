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
  internal class GraphClient : GraphServiceClient, IGraphServiceClient
  {
    protected readonly GraphHelperSettings settings;
    GraphClient(GraphHelperSettings settings)
      : base(new ClientCredentialProvider(ConfidentialClientApplicationBuilder
        .Create(settings.GraphAppClientId)
        .WithTenantId(settings.GraphAppTenantId)
        .WithClientSecret(settings.GraphAppClientSecret)
        .Build()))
    { }
  }
}
