// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CSE.Automation.Model.Commands
{
    internal class ServicePrincipalEvaluateCommand
    {
        public string CorrelationId { get; set; }
        public ServicePrincipalModel Model { get; set; }
    }
}
