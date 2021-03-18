// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CSE.Automation.Model
{
    internal class ActivityHistory
    {
        /// <summary>
        /// Gets or sets unique ObjectId of the document
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the correlation ObjectId of the activity
        /// </summary>
        public string CorrelationId { get; set; }

        public string Name { get; set; }

        public string CommandSource { get; set; }

        /// <summary>
        /// Gets or sets the status of the activity
        /// </summary>
        public ActivityHistoryStatus Status { get; set; }

        /// <summary>
        /// Gets or sets a message in case of a Status of Failed.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the metrics for the run
        /// </summary>
        public IDictionary<string, object> Metrics { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets timestamp of when the document was created
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// Gets or sets timestamp of when the document was last updated
        /// </summary>
        public DateTimeOffset LastUpdated { get; set; }

        public void MergeMetrics(IDictionary<string, object> dict)
        {
            Metrics = Metrics.Concat(dict).ToDictionary(x => x.Key, x => x.Value);
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    internal enum ActivityHistoryStatus
    {
        /// <summary>
        /// Activity is actively running
        /// </summary>
        Running,

        /// <summary>
        /// Activity has completed successfully.
        /// </summary>
        Completed,

        /// <summary>
        /// Activity has completed with errors
        /// </summary>
        Failed,
    }
}
