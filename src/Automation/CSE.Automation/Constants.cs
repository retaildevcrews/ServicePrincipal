// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
        public const string CosmosDBObjectTrackingCollectionName = "SPObjectTrackingCollection";
        public const string CosmosDBActivityHistoryCollectionName = "SPActivityHistoryCollection";

        // Azure Storage Queue Constants
        public const string SPStorageConnectionString = "SPStorageConnectionString";
        public const string DiscoverQueueAppSetting = "%SPDiscoverQueue%";
        public const string EvaluateQueueAppSetting = "%SPEvaluateQueue%";
        public const string UpdateQueueAppSetting = "%SPUpdateQueue%";

        // Azure Timer Function Constants
        public const string DeltaDiscoverySchedule = "%SPDeltaDiscoverySchedule%";

        //Queueing Constants
        public const int MaxVisibilityDelayGapSeconds = 500;
        public const int MaxQueueRecordProcessThreshold = 3000;

        // Validation Constants
        public const int MaxStringLength = 1000;
    }
}
