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
        public enum TypeFilter { all, servicePrincipal, user, application };

        //CosmosDB Constants
        public const string CosmosDBKeyName = "SPCosmosKey";
        public const string CosmosDBURLName = "SPCosmosURL";
        public const string CosmosDBDatabaseName = "SPCosmosDatabase";
        public const string CosmosDBConfigCollectionName = "SPConfigurationCollection";
        public const string CosmosDBAuditCollectionName = "SPAuditCollection";
        public const string CosmosDBOjbectTrackingCollectionName = "SPObjectTrackingCollection";

        //DAL Constants

    }
}
