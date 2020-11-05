using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CSE.Automation.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum DiscoveryMode
    {
        /// <summary>
        /// Execute a full AAD scan
        /// </summary>
        FullSeed,

        /// <summary>
        /// Execute a delta query
        /// </summary>
        Deltas,
    }

    internal class RequestDiscoveryCommand
    {
        public string CorrelationId { get; set; }
        public DiscoveryMode DiscoveryMode { get; set; }
    }
}
