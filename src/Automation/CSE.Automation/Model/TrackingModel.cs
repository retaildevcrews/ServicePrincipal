using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CSE.Automation.Model
{
    class TrackingModel
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

        public GraphModel Entity { get; set; }
    }
}
