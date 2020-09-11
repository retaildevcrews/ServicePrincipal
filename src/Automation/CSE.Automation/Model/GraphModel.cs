//using Microsoft.Graph;
//TODO: Microsoft.Graph has a Status Object so Idk if we want to call our status field something different
//to avoid namespace collision
using System;

namespace CSE.Automation.Model
{
    class GraphModel
    {
        public Guid Id { get; set; }

        //TODO: is this actually supposed to be "CreatedDateTime"?
        public DateTime Created { get; set; }

        public DateTime Deleted { get; set; }

        public DateTime LastUpdated { get; set; }

        public string ObjectType { get; set; }

        public Status Status { get; set; } 
    }

    public enum Status
    {
        Valid,
        Invalid,
        Deleted,
        Remediated
    }
}
