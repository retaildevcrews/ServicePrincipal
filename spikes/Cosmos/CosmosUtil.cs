using Microsoft.Azure.Cosmos;
using System;
using System.Configuration;
using Microsoft.Graph;
using System.Threading.Tasks;
using System.Net;

namespace Cosmos
{
    public class CosmosUtil
    {
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = ConfigurationManager.AppSettings["EndPointUri"];

        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = ConfigurationManager.AppSettings["PrimaryKey"];

        // The Cosmos client instance
        private readonly CosmosClient _cosmosClient;

        private readonly Database _database;

        private Container _container;

        private readonly string _databaseId = "ToDoList";

        private readonly string _containerId = "Items";

        public Task ContainerCreation { get; private set; }

        public CosmosUtil()
        {
            _cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            _database = _cosmosClient.GetDatabase(_databaseId);
            ContainerCreation = CreateContainerAsync();
        }

        private async Task CreateContainerAsync()
        {
            // Create a new container
            await _database.CreateContainerIfNotExistsAsync(_containerId, "/Id");
            Console.WriteLine("Created Container: {0}\n", _container.Id);
        }

        public async Task AddServicePrincipalToContainerAsync(ServicePrincipal servicePrincipal)
        {
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<ServicePrincipal> response = await _container.ReadItemAsync<ServicePrincipal>(servicePrincipal.Id, new PartitionKey(servicePrincipal.Id));
                Console.WriteLine("Item in database with id: {0} already exists\n", response.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
                ItemResponse<ServicePrincipal> response = await _container.CreateItemAsync<ServicePrincipal>(servicePrincipal, new PartitionKey(servicePrincipal.Id));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", response.Resource.Id, response.RequestCharge);
            }
        }
    }
}
