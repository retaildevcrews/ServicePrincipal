
using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;
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
        public IList<string> Owners { get; set; }

        public bool HasOwners()
        {
            return Owners != null && Owners.Count > 0;
        }
    }
}
