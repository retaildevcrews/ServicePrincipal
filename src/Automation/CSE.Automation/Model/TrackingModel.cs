using System;
using Newtonsoft.Json;

namespace CSE.Automation.Model
{
    internal class TrackingModel
    {
        public string Id { get; set; }

        // activity correlation id that last wrote this document
        public string CorrelationId { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset? Deleted { get; set; }

        public DateTimeOffset LastUpdated { get; set; }

        public ObjectType ObjectType { get; set; }

        public object Entity { get; set; }

        public static TEntity Unwrap<TEntity>(TrackingModel entity) where TEntity : class
        {
            return entity?.Entity as TEntity;
        }
    }

    internal class TrackingModel<TEntity> : TrackingModel where TEntity : GraphModel
    {
        [JsonIgnore]
        public TEntity TypedEntity
        {
            get => base.Entity is null
                    ? null
                    : JsonConvert.DeserializeObject<TEntity>(base.Entity.ToString());
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
