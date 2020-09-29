using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CSE.Automation.Model
{
    public class ProcessorConfiguration
    {
        public string Id { get; set; }

        public string FilterString { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ProcessorType ConfigType { get; set; }

        public List<string> SelectFields { get;  }

        public string DeltaLink { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RunState RunState { get; set; }

        public DateTime LastDeltaRun { get; set; }

        public DateTime LastSeedTime { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }

    public enum RunState
    {
        [EnumMember(Value = "seedOnly")]
        Seedonly,
        [EnumMember(Value = "seedAndRun")]
        SeedAndRun,
        [EnumMember(Value = "deltaRun")]
        DeltaRun,
        [EnumMember(Value = "disabled")]
        Disabled
    }
}
