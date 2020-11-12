// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

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

    // Make sure this serializes as an int
    public enum AuditCode
    {
        /// <summary>
        /// Code for a ServicePrincipal that passed all audit checks
        /// </summary>
        [Description("ServicePrincipal passed audit checks.")]
        Pass_ServicePrincipal = 0,

        /// <summary>
        /// Code for a ServicePrincipal that was ignored for being already deleted from AAD.
        /// </summary>
        [Description("ServicePrincipal ignored - already deleted.")]
        Ignore_ServicePrincipalDeleted = 1,

        /// <summary>
        /// Code for a ServicePrincipal that was updated in AAD.
        /// </summary>
        [Description("ServicePrincipal updated in AAD.")]
        Change_ServicePrincipalUpdated = 100,

        /// <summary>
        /// Code for an attribute validation error
        /// </summary>
        [Description("Attribute validation failure.")]
        Fail_AttributeValidation = -1,

        /// <summary>
        /// Code for a ServicePrincipal missing Owner values.
        /// </summary>
        [Description("ServicePrincipal is missing Owners.")]
        Fail_MissingOwners = -2,

        /// <summary>
        /// Code when AAD fails to update.
        /// </summary>
        [Description("Failed to update ServicePrincipal field: {0}.")]
        Fail_AADUpdate = -3,
    }

    public class AuditEntry
    {
        public string Id { get; set; }
        public string CorrelationId { get; set; }

        public string ObjectId { get; set; }

        public AuditActionType Type { get; set; }

        public int Code { get; set; }

        public string Reason { get; set; }

        public string Message { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public string AuditYearMonth { get; set; }

        public string AttributeName { get; set; }

        public string ExistingAttributeValue { get; set; }

        public string UpdatedAttributeValue { get; set; }
    }
}
