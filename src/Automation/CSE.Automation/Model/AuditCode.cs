// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CSE.Automation.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum AuditCode
    {
        /// <summary>
        /// Code for a ServicePrincipal that passed all audit checks
        /// </summary>
        [Description("ServicePrincipal passed audit checks.")]
        Pass = 0,

        /// <summary>
        /// Code for a ServicePrincipal that was ignored for being already deleted from AAD.
        /// </summary>
        [Description("ServicePrincipal ignored - already deleted.")]
        Deleted = 1,

        /// <summary>
        /// Code for a ServicePrincipal that was updated in AAD.
        /// </summary>
        [Description("ServicePrincipal updated in AAD.")]
        Updated = 100,

        /// <summary>
        /// Code for an attribute validation error
        /// </summary>
        [Description("Attribute validation failure.")]
        AttributeValidation = -1,

        /// <summary>
        /// Code for a ServicePrincipal missing Owner values in the extension field (Notes).
        /// </summary>
        [Description("ServicePrincipal is missing Owners in extention field (Notes).")]
        MissingOwners = -2,

        /// <summary>
        /// Code when AAD fails to update.
        /// </summary>
        [Description("Failed to update ServicePrincipal field: {0}.")]
        AADUpdate = -3,
    }
}
