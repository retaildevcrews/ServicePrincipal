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
        public string AppId { get; set; }
        public string ServicePrincipalType { get; set; }

        public string AppDisplayName { get; set; }

        public string DisplayName { get; set; }

        public string Notes { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
        public List<string> Owners { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        public bool HasOwners()
        {
            return Owners != null && Owners.Count > 0;
        }
    }
}
