using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CSE.Automation.Model
{
    internal class ActivityHistory
    {
        /// <summary>
        /// Gets or sets unique Id of the document
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the correlation Id of the activity 
        /// </summary>
        public string CorrelationId { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the status of the activity
        /// </summary>
        public ActivityHistoryStatus Status { get; set; }

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
