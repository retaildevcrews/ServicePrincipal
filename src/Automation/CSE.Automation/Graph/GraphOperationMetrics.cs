// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace CSE.Automation.Graph
{
    public class GraphOperationMetrics
    {
        public string Name { get; set; }
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
