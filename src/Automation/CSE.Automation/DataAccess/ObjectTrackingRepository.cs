// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace CSE.Automation.DataAccess
{
    internal class ObjectTrackingRepository : CosmosDBRepository<TrackingModel>, IObjectTrackingRepository
    {
        private readonly ObjectTrackingRepositorySettings settings;
        public ObjectTrackingRepository(ObjectTrackingRepositorySettings settings, ILogger<ObjectTrackingRepository> logger)
        : base(settings, logger)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Generate an id for the tracking model.
        /// </summary>
        /// <param name="trackingModel">An instance of the tracking model</param>
        /// <returns>The id of the tracking model</returns>
        /// <remarks>The id of the tracking model must be the same as the wrapped entity.  The entity's id is projected to the outer wrapper.</remarks>
        public override string GenerateId(TrackingModel trackingModel)
        {
            var entity = trackingModel.Entity as GraphModel;
            if (string.IsNullOrWhiteSpace(entity?.Id))
            {
                trackingModel.Id = Guid.NewGuid().ToString();
            }

            return trackingModel.Id;
        }

        public override string CollectionName => this.settings.CollectionName;
    }
}
