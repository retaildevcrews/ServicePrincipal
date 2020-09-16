using System;

namespace CSE.Automation.Model
{
    public interface IGraphModel
    {
        public string Id { get; set; }

        public DateTime Created { get; set; }

        public DateTime Deleted { get; set; }

        public DateTime LastUpdated { get; set; }

        public ObjectType ObjectType { get; set; }

        public Status Status { get; set; }
    }
}
