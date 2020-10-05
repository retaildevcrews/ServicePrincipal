using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CSE.Automation.DataAccess
{
    interface IConfigRepository{}
    class ConfigRepository : CosmosDBRepository, IConfigRepository
    {
        public ConfigRepository(ICosmosDBSettings settings, ILogger<ConfigRepository> logger) : base(settings, logger)
        {
        }

        public override string GenerateId<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public override string CollectionName  => Constants.CosmosDBConfigCollectionName;
    }
}
