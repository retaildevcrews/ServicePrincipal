// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CSE.Automation.Model
{
    internal enum ServicePrincipalUpdateAction
    {
        /// <summary>
        /// Update Notes
        /// </summary>
        [Description("Update Notes from Owners.")]
        Update,

        /// <summary>
        /// Revert
        /// </summary>
        [Description("Revert to Last Known Good.")]
        Revert,
    }

    internal class ServicePrincipalUpdateCommand
    {
        public string CorrelationId { get; set; }
        public string ObjectId { get; set; }
        public string AppId { get; set; }
        public string DisplayName { get; set; }
        public (string Current, string Changed) Notes { get; set; }
        public ServicePrincipalUpdateAction Action { get; set; }
    }
}
