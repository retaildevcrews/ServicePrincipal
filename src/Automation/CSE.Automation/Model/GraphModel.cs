using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CSE.Automation.Model
{
    public class GraphModel : IGraphModel
    {
        public string Id { get; set; }

        public DateTime Created { get; set; }

        public DateTime Deleted { get; set; }

        public DateTime LastUpdated { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ObjectType ObjectType { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Status Status { get; set; }
    }

    public enum ObjectType
    {
        [EnumMember(Value = "servicePrincipal")]
        ServicePrincipal
    }

    public enum Status
    {
        [EnumMember(Value = "valid")]
        Valid,
        [EnumMember(Value = "invalid")]
        Invalid,
        [EnumMember(Value = "deleted")]
        Deleted,
        [EnumMember(Value = "remediated")]
        Remediated
    }
}
