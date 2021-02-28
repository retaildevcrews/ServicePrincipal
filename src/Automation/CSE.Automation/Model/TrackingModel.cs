// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace CSE.Automation.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum TrackingState
    {
        /// <summary>
        /// State for a tracked entity
        /// </summary>
        [Description("Entity is currently tracked.")]
        Tracked,

        /// <summary>
        /// State for an untracked entity
        /// </summary>
        [Description("Entity is deleted and no longer tracked.")]
        Untracked,
    }

    internal class TrackingModel
    {
        public string Id { get; set; }

        // activity correlation id that last wrote this document
        public string CorrelationId { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset? Deleted { get; set; }

        public DateTimeOffset LastUpdated { get; set; }

        public ObjectType ObjectType { get; set; }

        public TrackingState State { get; set; } = TrackingState.Tracked;

        public object Entity { get; set; }

        public static TEntity Unwrap<TEntity>(TrackingModel entity)
        where TEntity : class
        {
            JObject entityAsJObject = entity?.Entity as JObject;

            string entityAsString = entityAsJObject?.ToString(Formatting.None);

            return string.IsNullOrEmpty(entityAsString) ? null : JsonConvert.DeserializeObject<TEntity>(entityAsString);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Colocation of functionality")]
    internal class TrackingModel<TEntity> : TrackingModel
        where TEntity : GraphModel
    {
        [JsonIgnore]
        public TEntity TypedEntity
        {
            get
            {
                return this.Entity is null
                    ? null
                    : JsonConvert.DeserializeObject<TEntity>(this.Entity.ToString());
            }
            set
            {
                this.Entity = value;
                if (value != null)
                {
                    this.Id = value.Id;
                }
            }
        }
    }
}
