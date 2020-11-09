// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace CSE.Automation.DataAccess
{
    internal class ActivityHistoryRepository : CosmosDBRepository<ActivityHistory>, IActivityHistoryRepository
    {
        private readonly ActivityHistoryRepositorySettings settings;
        public ActivityHistoryRepository(ActivityHistoryRepositorySettings settings, ILogger<AuditRepository> logger)
            : base(settings, logger)
        {
            this.settings = settings;
        }

        public override string GenerateId(ActivityHistory entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString();
            }

            return entity.Id;
        }

        public async Task<IEnumerable<ActivityHistory>> GetCorrelated(string correlationId)
        {
            return await InternalCosmosDBSqlQuery($"select * from c where c.correlationId = '{correlationId}' order by c.created").ConfigureAwait(false);
        }

        public override string CollectionName => settings.CollectionName;
    }
}
