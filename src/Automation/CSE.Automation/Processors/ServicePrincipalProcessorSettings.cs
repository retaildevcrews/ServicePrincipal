// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Configuration;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Azure.Storage.Queue;

namespace CSE.Automation.Processors
{
    internal class ServicePrincipalProcessorSettings : DeltaProcessorSettings, IServicePrincipalProcessorSettings
    {
        private string queueConnectionString;
        private string evaluateQueueName;
        private string updateQueueName;
        private string discoverQueueName;
        public ServicePrincipalProcessorSettings(ISecretClient secretClient)
            : base(secretClient) { }

        [Secret(Constants.SPStorageConnectionString)]
        public string QueueConnectionString
        {
            get { return queueConnectionString ?? GetSecret(); }
            set { queueConnectionString = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "Not a localizable setting")]
        public string EvaluateQueueName
        {
            get { return evaluateQueueName; }
            set { evaluateQueueName = value?.ToLower(); }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "Not a localizable setting")]
        public string UpdateQueueName
        {
            get { return updateQueueName; }
            set { updateQueueName = value?.ToLower(); }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "Not a localizable setting")]
        public string DiscoverQueueName
        {
            get { return discoverQueueName; }
            set { discoverQueueName = value?.ToLower(); }
        }

        public UpdateMode AADUpdateMode { get; set; }

        public UpdateField UpdateField { get; set; } = UpdateField.Notes;

        public override void Validate()
        {
            base.Validate();
            if (string.IsNullOrEmpty(this.QueueConnectionString))
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: QueueConnectionString is invalid");
            }

            if (string.IsNullOrEmpty(this.EvaluateQueueName))
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: EvaluateQueueName is invalid");
            }

            if (string.IsNullOrEmpty(this.UpdateQueueName))
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: UpdateQueueName is invalid");
            }

            if (string.IsNullOrEmpty(this.DiscoverQueueName))
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: DiscoverQueueName is invalid");
            }
        }
    }
}
