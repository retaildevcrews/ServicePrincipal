// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;

namespace CSE.Automation.Services
{
    internal class ServicePrincipalClassifier : IServicePrincipalClassifier
    {
        public async Task<ServicePrincipalClassification> Classify(ServicePrincipalClassification entity)
        {
            return await Task.FromResult((ServicePrincipalClassification)null).ConfigureAwait(false);
        }
    }
}
