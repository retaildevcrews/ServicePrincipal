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

        public override string GenerateId(TrackingModel entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString();
            }

            return entity.Id;
        }

        public override string CollectionName => this.settings.CollectionName;
    }
}
