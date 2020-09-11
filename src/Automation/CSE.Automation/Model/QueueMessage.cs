namespace CSE.Automation.Model
{
    class QueueMessage
    {
        public QueueMessageType QueueMessageType { get; set;}

        public int Attempt { get; set; }

        //TODO: string or enum? 
        public string Document { get; set; }
    }

    public enum QueueMessageType
    {
        Data,
        Audit
    }
}
