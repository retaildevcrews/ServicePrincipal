// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using CSE.Automation.Model;
using CSE.Automation.Model.Commands;

namespace CSE.Automation.Interfaces
{
    internal interface IServicePrincipalProcessor : IDeltaProcessor
    {
        Task Evaluate(ActivityContext context, ServicePrincipalModel entity);
        Task UpdateServicePrincipal(ActivityContext context, ServicePrincipalUpdateCommand command);
    }
}
