// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace CSE.Automation.DataAccess
{
    internal class AuditRepositorySettings : CosmosDBSettings
    {
        public AuditRepositorySettings(ISecretClient secretClient)
            : base(secretClient)
        {
        }

        public string CollectionName { get; set; }
        public override void Validate()
        {
            base.Validate();
            if (string.IsNullOrEmpty(this.CollectionName))
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: CollectionName is invalid");
            }
        }
    }
}
