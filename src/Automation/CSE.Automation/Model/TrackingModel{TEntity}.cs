// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;

namespace CSE.Automation.Model
{
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
