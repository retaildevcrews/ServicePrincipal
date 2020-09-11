using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CSE.Automation.Model
{
    class Configuration
    {
        public Guid ProcessorId { get; set; }

        public string FilterString { get; set; }

        public string SelectFields { get; set; }

        public string DeltaLink { get; set; }
        //TODO: Can we define these conversions, camelCase, enum, array converters, etc in startup?
        //same for below code
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RunState RunState { get; set; }

        public DateTime LastDeltaRun { get; set; }

        public DateTime LastSeedTime { get; set; }
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
