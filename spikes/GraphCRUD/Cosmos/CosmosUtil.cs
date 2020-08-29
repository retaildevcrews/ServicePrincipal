using Microsoft.Azure.Cosmos;
using Microsoft.Graph;
using System;
using System.Configuration;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace GraphCrud
{
    public sealed class CosmosUtil : ICosmosUtil
    {
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = ConfigurationManager.AppSettings["EndPointUri"];

        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = ConfigurationManager.AppSettings["PrimaryKey"];

        private static CosmosUtil _instance = null;

        //Creating thread-safe singleton via double lock method with semaphore
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        // The Cosmos client instance
        private CosmosClient _cosmosClient;

        private Database _database;

        private Container _container;

        private readonly string _databaseId = ConfigurationManager.AppSettings.Get("DatabaseId");

        private readonly string _containerId = ConfigurationManager.AppSettings.Get("ContainerId");

        private static int _instanceCounter = 0;

        private CosmosUtil() { _instanceCounter++; }

        private async Task InitializeAsync()
        {
            _cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            _database = await CreateDatabaseAsync();
            Console.WriteLine("Either Created Or Already Exists Database: {0}\n", _database.Id);
            _container = await CreateContainerAsync();
            Console.WriteLine("Either Created Or Already Exists Container: {0}\n", _container.Id);
        }

        public static async Task<CosmosUtil> CreateAsync()
        {
            if (_instance == null)
            {
                await semaphoreSlim.WaitAsync();
                try
                {
                    if (_instance == null)
                    {
                        _instance = new CosmosUtil();
                        await _instance.InitializeAsync();
                    }
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
            Console.WriteLine($"instance count: {_instanceCounter}");
            return _instance;
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
