using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ChangeFeed
{
    public static class cosmosdbTrigger
    {
        [FunctionName("cosmosdbTrigger")]
        public static void Run([CosmosDBTrigger(
            databaseName: "Demo",
            collectionName: "ServicePrincipal",
            ConnectionStringSetting = "DBConnection",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents inserted/modified " + input.Count);

                foreach (Document doc in input)
                {
                    log.LogInformation(doc.ToString());
                }           
                
            }
        }
    }
}
