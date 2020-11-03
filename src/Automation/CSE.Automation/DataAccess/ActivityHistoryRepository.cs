using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
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

        public override string CollectionName => settings.CollectionName;
    }
}
