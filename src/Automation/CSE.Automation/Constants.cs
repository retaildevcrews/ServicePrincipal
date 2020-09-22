namespace CSE.Automation
{
    public sealed class Constants
    {
        //secure execution constants
        public const string AuthType = "AUTH_TYPE";
        public const string KeyVaultName = "KEYVAULT_NAME";

        //Graph API constants
        public const string GraphAppClientIdKey = "graphAppClientId";
        public const string GraphAppTenantIdKey = "graphAppTenantId";
        public const string GraphAppClientSecretKey = "graphAppClientSecret";

        //CosmosDB Constants
        public const int DefaultPageSize = 100;
        public const int MaxPageSize = 1000;

        public const string CosmosDBKeyName = "SPCosmosKey";
        public const string CosmosDBURLName = "SPCosmosURL";
        public const string CosmosDBDatabaseName = "SPCosmosDatabase";
        public const string CosmosDBConfigCollectionName = "SPConfigurationCollection";
        public const string CosmosDBAuditCollectionName = "SPAuditCollection";
        public const string CosmosDBOjbectTrackingCollectionName = "SPObjectTrackingCollection";

        // Azure Storage Queue Constants
        public const string SPStorageConnectionString = "SPStorageConnectionString";
        public const string SPTrackingUpdateQueue = "SPTrackingUpdateQueue"; 
        public const string SPAADUpdateQueue = "SPAADUpdateQueue";

        //DAL Constants

    }
}
