// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<string> Owners { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        public bool HasOwners()
        {
            return Owners != null && Owners.Count > 0;
        }
    }
}
