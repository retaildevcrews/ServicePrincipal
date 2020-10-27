using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;


namespace CSE.Automation.Model
{
    public class GraphModel : IGraphModel
    {
        public string Id { get; set; }

        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Deleted { get; set; }
        public DateTimeOffset? LastUpdated { get; set; }

        public ObjectType ObjectType { get; set; }

        public Status Status { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ObjectType
    {
        ServicePrincipal
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Status
    {
        Valid,
        Invalid,
        Deleted,
        Remediated
    }
}
