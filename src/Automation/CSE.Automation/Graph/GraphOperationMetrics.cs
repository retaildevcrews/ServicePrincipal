// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;

namespace CSE.Automation.Graph
{
    public enum GraphOperation
    {
        /// <summary>
        /// Execute a full AAD scan
        /// </summary>
        [Description("Full Seed")]
        FullSeed,

        /// <summary>
        /// Execute a delta query
        /// </summary>
        [Description("Delta Discovery")]
        DeltaDiscovery,
    }

    public class GraphOperationMetrics
    {
        public GraphOperation Name { get; set; }
        public int Considered { get; set; }
        public int Removed { get; set; }
        public int Found { get; set; }
        public string AdditionalData { get; set; }

        public IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>()
            {
                { "Considered", Considered },
                { "Removed", Removed },
                { "Found", Found },
                { "AdditionalData", AdditionalData },
            };
        }
    }
}
