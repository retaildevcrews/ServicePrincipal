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
    internal class ActivityHistoryRepositorySettings : CosmosDBSettings
    {
        public ActivityHistoryRepositorySettings(ISecretClient secretClient)
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
