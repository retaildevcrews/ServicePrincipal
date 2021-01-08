// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CSE.Automation.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AuditActionType
    {
        /// <summary>
        /// PASS audit action
        /// </summary>
        Pass,

        /// <summary>
        /// FAIL audit action
        /// </summary>
        Fail,

        /// <summary>
        /// CHANGE audit action
        /// </summary>
        Change,

        /// <summary>
        /// IGNORE audit action
        /// </summary>
        Ignore,
    }
}
