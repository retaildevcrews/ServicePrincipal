using System;

namespace CSE.Automation.Model
{
    public interface IGraphModel
    {
        public string Id { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset? Deleted { get; set; }

        public DateTimeOffset? LastUpdated { get; set; }

        public ObjectType ObjectType { get; set; }
    }
}
