// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CSE.Automation.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum ObjectClassification
    {
        /// <summary>
        /// Directory Object is attributed to a Microsoft tenant.
        /// </summary>
        [Description("Microsoft tenanted object")]
        Microsoft,

        /// <summary>
        /// Directory Object is attributed to an external tenant.
        /// </summary>
        [Description("External tenanted object")]
        External,

        /// <summary>
        /// Directory Object is attributed to the current tenant.
        /// </summary>
        [Description("Company tenanted object")]
        Company,
    }
}
