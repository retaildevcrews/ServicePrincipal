using Newtonsoft.Json;
using System;

namespace CSE.Automation.Model
{
    class TrackingModel
    {
        public string Id { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset? Deleted { get; set; }

        public DateTimeOffset LastUpdated { get; set; }

        public ObjectType ObjectType { get; set; }

        public object Entity { get; set; }
    }

    class TrackingModel<TEntity> : TrackingModel where TEntity : GraphModel
    {
        [JsonIgnore]
        public TEntity TypedEntity
        {
            get { 
                return base.Entity is null
                    ? null
                    : JsonConvert.DeserializeObject<TEntity>(base.Entity.ToString());
            }
            set
            {
                base.Entity = value;
                if (value != null)
                {
                    base.Id = value.Id;
                }
            }
        }
    }
}
