using System.Collections.Generic;
using Newtonsoft.Json;

namespace CSE.Automation.Model
{
    // Used name ServicePrincipalModel to disambiguate from Microsoft.Graph.ServicePrincipal
    public class ServicePrincipalModel : GraphModel, IGraphModel
    {
        [JsonProperty(PropertyName = "appId")]
        public string AppId { get; set; }

        [JsonProperty(PropertyName = "appDisplayName")]
        public string AppDisplayName { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "notes")]
        public string Notes { get; set; }

        [JsonProperty(PropertyName = "owners")]
        public IEnumerable<string> Owners { get; set; }
    }
}
