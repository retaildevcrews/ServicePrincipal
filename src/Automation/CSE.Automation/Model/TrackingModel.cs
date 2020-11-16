// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

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

        public static TEntity Unwrap<TEntity>(TrackingModel entity)
        where TEntity : class
        {
            JObject entityAsJObject = entity?.Entity as JObject;

            string entityAsString = entityAsJObject?.ToString(Formatting.None);

            return string.IsNullOrEmpty(entityAsString) ? null : JsonConvert.DeserializeObject<TEntity>(entityAsString);
        }
    }
}
