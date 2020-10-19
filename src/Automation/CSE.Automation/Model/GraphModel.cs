using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CSE.Automation.Model
{
    public class GraphModel : IGraphModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "created")]
        public DateTimeOffset Created { get; set; }
        [JsonProperty(PropertyName = "deleted")]
        public DateTimeOffset? Deleted { get; set; }
        [JsonProperty(PropertyName = "lastUpdated")]
        public DateTimeOffset LastUpdated { get; set; }

        [JsonProperty(PropertyName = "objectType")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public ObjectType ObjectType { get; set; }

        [JsonProperty(PropertyName = "status")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
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
