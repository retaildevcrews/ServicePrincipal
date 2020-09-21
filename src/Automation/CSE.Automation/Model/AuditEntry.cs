using System;

namespace CSE.Automation.Model
{
    public class AuditEntry<T> : GraphModel
        where T : IGraphModel
    {
        public string CorrelationId { get; set; }

        public string ActionType { get; set; }

        public string ActionReason { get; set; }

        public DateTime ActionDateTime { get; set; }

        public T OriginalDocument { get; set; }

        public AuditEntry(IGraphModel originalDocument)
        {
            this.Id = originalDocument.Id;
            this.Created = originalDocument.Created;
            this.Deleted = originalDocument.Deleted;
            this.LastUpdated = originalDocument.LastUpdated;
            this.ObjectType = originalDocument.ObjectType;
            this.Status = originalDocument.Status;

            this.OriginalDocument = (T)originalDocument;
        }


    }
}
