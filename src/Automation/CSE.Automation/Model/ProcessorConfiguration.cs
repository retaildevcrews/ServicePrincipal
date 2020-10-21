using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CSE.Automation.Model
{
    public class ProcessorConfiguration
    {
        public string Id { get; set; }

        public string FilterString { get; set; }

        public ProcessorType ConfigType { get; set; }

        public List<string> SelectFields { get; set; }

        public string DeltaLink { get; set; }

        public RunState RunState { get; set; }

        public DateTimeOffset? LastDeltaRun { get; set; }

        public DateTimeOffset? LastSeedTime { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RunState
    {
        Seedonly,
        SeedAndRun,
        DeltaRun,
        Disabled
    }
}
