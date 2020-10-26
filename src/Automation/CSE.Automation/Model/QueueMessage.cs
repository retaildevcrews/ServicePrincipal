using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CSE.Automation.Model
{
    public abstract class QueueMessage
    {
        public QueueMessageType QueueMessageType { get; set; }

        public int Attempt { get; set; }

    }
    public class QueueMessage<TDocument> : QueueMessage
    {
        public TDocument Document { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum QueueMessageType
    {
        Data,
        Audit
    }
}
