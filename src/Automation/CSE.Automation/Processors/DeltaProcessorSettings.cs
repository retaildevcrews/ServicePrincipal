using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using CSE.Automation.Properties;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using SettingsBase = CSE.Automation.Model.SettingsBase;
using Microsoft.Identity.Client;
using System.Net.WebSockets;
using CSE.Automation.Graph;
using Microsoft.Extensions.Logging;

namespace CSE.Automation.Processors
{
    public class DeltaProcessorSettings : SettingsBase
    {
        public DeltaProcessorSettings(ISecretClient secretClient)
                : base(secretClient)
        {
        }

        public Guid ConfigurationId { get; set; }
        public int VisibilityDelayGapSeconds { get; set; }
        public int QueueRecordProcessThreshold { get; set; }

        public override void Validate()
        {
            base.Validate();
            if (this.ConfigurationId == Guid.Empty)
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: ConfigurationId is invalid");
            }

            if (this.VisibilityDelayGapSeconds <= 0 || this.VisibilityDelayGapSeconds > Constants.MaxVisibilityDelayGapSeconds)
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: VisibilityDelayGapSeconds is invalid");
            }

            if (this.QueueRecordProcessThreshold <= 0 || this.QueueRecordProcessThreshold > Constants.MaxQueueRecordProcessThreshold)
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: QueueRecordProcessThreshold is invalid");
            }
        }
    }
}
