using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CSE.Automation.Model
{
    public class ProcessorConfiguration
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "filterString")]
        public string FilterString { get; set; }

        [JsonProperty(PropertyName = "configType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ProcessorType ConfigType { get; set; }

        [JsonProperty(PropertyName = "selectFields")]
        public List<string> SelectFields { get; set; }

        [JsonProperty(PropertyName = "deltaLink")]
        public string DeltaLink { get; set; }

        [JsonProperty(PropertyName = "runState")]
        [JsonConverter(typeof(StringEnumConverter))]
        public RunState RunState { get; set; }

        [JsonProperty(PropertyName = "lastDeltaRun")]
        public DateTime? LastDeltaRun { get; set; }

        [JsonProperty(PropertyName = "lastSeedTime")]
        public DateTime? LastSeedTime { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
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
