using Microsoft.Azure.Cosmos;
using System;
using System.Configuration;
using Microsoft.Graph;
using System.Threading.Tasks;
using System.Net;

namespace GraphCrud
{
    public sealed class CosmosUtil
    {
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = ConfigurationManager.AppSettings["EndPointUri"];

        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = ConfigurationManager.AppSettings["PrimaryKey"];

        // The Cosmos client instance
        private CosmosClient _cosmosClient;

        private Database _database;

        private Container _container;

        private readonly string _databaseId = "SeanSPDB";

        private readonly string _containerId = "ServicePrincipals";

        private CosmosUtil(){}

        private async Task<CosmosUtil> InitializeAsync()
        {
            _cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            _database = await CreateDatabaseAsync();
            Console.WriteLine("Either Created Or Already Exists Database: {0}\n", _database.Id);
            _container = await CreateContainerAsync();
            Console.WriteLine("Either Created Or Already Exists Container: {0}\n", _container.Id);
            return this;
        }

        public static Task<CosmosUtil> CreateAsync()
        {

            var cosmosUtil = new CosmosUtil();
            return cosmosUtil.InitializeAsync();
        }

        private async Task<Database> CreateDatabaseAsync()
        {
            // Create a new database
            var databaseResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseId);
            var database = databaseResponse.Database;
            return database;
        }

        private async Task<Container> CreateContainerAsync()
        {
            // Create a new container
            var containerResponse = await _database.CreateContainerIfNotExistsAsync(_containerId, "/displayName", 400);
            var container = containerResponse.Container;
            return container;
        }

        public async Task AddServicePrincipalToContainerAsync(ServicePrincipal servicePrincipal)
        {
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<ServicePrincipal> response = await _container.ReadItemAsync<ServicePrincipal>(servicePrincipal.DisplayName, new PartitionKey(servicePrincipal.DisplayName));
                Console.WriteLine("Item in database with id: {0} already exists\n", response.Resource.DisplayName);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
                ItemResponse<ServicePrincipal> response = await _container.CreateItemAsync<ServicePrincipal>(servicePrincipal, new PartitionKey(servicePrincipal.DisplayName));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", response.Resource.DisplayName, response.RequestCharge);
            }
        }
    }
}
