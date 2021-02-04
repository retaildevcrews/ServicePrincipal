// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using CSE.Automation.Interfaces;
using Microsoft.Graph;

namespace CSE.Automation.Graph
{
    internal interface IServicePrincipalGraphHelper : IGraphHelper<ServicePrincipal>
    {
        Task<Application> GetApplicationWithOwners(string appId);
    }
}
