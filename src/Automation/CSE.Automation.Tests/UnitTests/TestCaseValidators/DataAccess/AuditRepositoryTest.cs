using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CSE.Automation.DataAccess;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.DataAccess
{

    internal class AuditRepositoryTest : CosmosDBRepository<AuditEntry>, IAuditRepository
    {
        private readonly AuditRepositorySettings settings;
        public AuditRepositoryTest(AuditRepositorySettings settings, ILogger<AuditRepository> logger)
            : base(settings, logger)
        {
            this.settings = settings;
        }

        public override string GenerateId(AuditEntry entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString();
            }

            return entity.Id;
        }

        public override string CollectionName => settings.CollectionName;


        public async Task<IEnumerable<AuditEntry>> GetMostRecentAsync(string objectId, int limit = 1)
        {
            Guid guidValue;
            try
            {
                guidValue = Guid.Parse(objectId); // to prevent SQL injection attack
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                throw new InvalidDataException($"Invalid GUID value {objectId}", ex);
            }

            string sql = $"SELECT * FROM c WHERE c.descriptor.objectId = '{objectId}' ORDER BY c._ts DESC OFFSET 0 LIMIT {limit}";

            return await InternalCosmosDBSqlQuery(sql).ConfigureAwait(false);
        }

        public async Task<IEnumerable<AuditEntry>> GetItemsAsync(string objectId, string correlationId)
        {
            Guid guidValue;
            try
            {
                guidValue = Guid.Parse(objectId); // to prevent SQL injection attack

                guidValue = Guid.Parse(correlationId); // to prevent SQL injection attack
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                throw new InvalidDataException($"Invalid GUID value(s) '{objectId}' , '{correlationId}'", ex);
            }

            string sql = $"SELECT * FROM c where c.descriptor.correlationId = '{correlationId}' and c.descriptor.objectId = '{objectId}' ORDER BY c._ts DESC";

            return await InternalCosmosDBSqlQuery(sql).ConfigureAwait(false);
        }


        public async Task<int> GetCountAsync(string objectId, string correlationId)
        {
            Guid guidValue;
            try
            {
                guidValue = Guid.Parse(objectId); // to prevent SQL injection attack

                guidValue = Guid.Parse(correlationId); // to prevent SQL injection attack
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                throw new InvalidDataException($"Invalid GUID value(s) '{objectId}' , '{correlationId}'", ex);
            }

            string sql = $"SELECT VALUE COUNT(1) FROM c where c.descriptor.correlationId = '{correlationId}' and c.descriptor.objectId = '{objectId}'";


            var query = this.GetPrivateContainerInstance().GetItemQueryIterator<int>(sql);

            int result = 0;

            if (query.HasMoreResults)
            {
                var countResponse = await query.ReadNextAsync().ConfigureAwait(false);
                result = countResponse.Resource.Any() ? countResponse.Resource.First() : 0;
            }

            return result;
        }


        private Container GetPrivateContainerInstance()
        {
            Container  result = (Container)this.GetFieldValue("Container");

            return result;

        }

        private PropertyInfo GetPropertyInfo(Type type, string fieldName)
        {
            PropertyInfo fieldInfo;
            do
            {
                fieldInfo = type.GetProperty(fieldName,
                       BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                type = type.BaseType;
            }
            while (fieldInfo == null && type != null);
            return fieldInfo;
        }

        public object GetFieldValue(string fieldName)
        {

            Type objType = this.GetType();
            PropertyInfo fieldInfo = GetPropertyInfo(objType, fieldName);
            if (fieldInfo == null)
                throw new ArgumentOutOfRangeException("fieldName",
                  string.Format("Couldn't find field {0} in type {1}", fieldName, objType.FullName));
            return fieldInfo.GetValue(this);
        }
    }
}
