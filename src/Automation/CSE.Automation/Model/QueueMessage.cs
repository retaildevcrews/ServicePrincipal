using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CSE.Automation.Model
{
    
    public class QueueMessage
    {        
        public QueueMessageType QueueMessageType { get; set;}

        public int Attempt { get; set; }

        public string Document { get; set; }
    }

    public enum QueueMessageType
    {
        [EnumMember(Value = "data")]
        Data,
        [EnumMember(Value = "audit")]
        Audit
    }
}
