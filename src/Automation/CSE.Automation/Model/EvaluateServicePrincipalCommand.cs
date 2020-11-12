// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation.Model
{
    internal class EvaluateServicePrincipalCommand
    {
        public string CorrelationId { get; set; }
        public ServicePrincipalModel Model { get; set; }
    }
}
