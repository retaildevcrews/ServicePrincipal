using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using System.Threading.Tasks;

namespace CosmosDBTool
{
    class CosmosDBManager : IDisposable
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Database _database;
        private readonly CosmosDBSettings _cosmosDBSettings;

        private List<Container> _containerList = new List<Container>();
        private Dictionary<string, string> _currentContainerNamePrimaryKey = new Dictionary<string, string>();

        private List<string> _logger = new List<string>();

        public string LogFileName { get; set; }
        public CosmosDBManager(CosmosDBSettings cosmosDBSettings)
        {
            _cosmosDBSettings = cosmosDBSettings;
            _cosmosClient = new CosmosClient(_cosmosDBSettings.Endpoint, _cosmosDBSettings.AuthKey);
            _database = _cosmosClient.GetDatabase(_cosmosDBSettings.DatabaseName);

            InitializeContainers();
        }

        private void InitializeContainers()
        {
           foreach(var containerName in _cosmosDBSettings.ContainerNames)
            {  
                //Container proxy reference doesn't guarantee existence.
                var container = _cosmosClient.GetContainer(_cosmosDBSettings.DatabaseName, containerName);

                if (container != null)
                {
                    _containerList.Add(container);
                }
            }
        }

        public void RecreateContainers()
        {
            DeleteContainers();
            CreateContainers();
            ShowLog();
        }

        private void ShowLog()
        {
            if (_logger.Count > 0)
            {
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"));

                LogFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_Execution.log");
                File.WriteAllLines(LogFileName, _logger);
                Task.Delay(500);
            }
        }

        private void GetCurrentContainerPartitionKeys()
        {
            ConsoleHelper.UpdateConsole("Getting Partition Keys....", _logger);
            _currentContainerNamePrimaryKey = new Dictionary<string, string>();
            
            //foreach (var targetContainer in _containerList)
            Parallel.ForEach(_containerList, targetContainer =>
            {
                try
                {
                    var containerProperties = GetContainerProperties(targetContainer).Result;
                    var partitionKeyName = containerProperties.PartitionKeyPath.TrimStart('/');

                    _currentContainerNamePrimaryKey.Add(targetContainer.Id, partitionKeyName);
                    _logger.Add($"Partition Key name [{partitionKeyName}] was successfully retrieved from  Container [{targetContainer.Id}]");
                }
                catch (Exception ex)
                {
                    if (HandleCosmosException(ex))
                    {
                        //use configuration to get the Peimary Key
                        var partitionKeyValue = _cosmosDBSettings.ContainerNamePrimaryKey.FirstOrDefault(x => x.Key == targetContainer.Id);

                        _currentContainerNamePrimaryKey.Add(targetContainer.Id, partitionKeyValue.Value);

                        _logger.Add($"Partition Key name [{partitionKeyValue.Value}] was read from config file for Container [{targetContainer.Id}]");
                    }
                    else
                    {
                        _logger.Add($"Unexpected error occured when Resolving Primary Key for container [{targetContainer.Id}].");
                    }

                }
            });

        }

        private async Task<ContainerProperties> GetContainerProperties(Container container = null)
        {
            return (await container.ReadContainerAsync().ConfigureAwait(false)).Resource;
        }

        private bool DeleteContainers()
        {
            GetCurrentContainerPartitionKeys();

            ConsoleHelper.UpdateConsole("Deleting Containers....", _logger);


            Parallel.ForEach(_containerList, targetContainer =>
            {

                //Container proxy reference doesn't guarantee existence. soif container does not exist it will throw an exception.
                Task<ContainerResponse> deleteTask = Task.Run(() => targetContainer.DeleteContainerAsync());
                try
                {
                    deleteTask.Wait();
                    
                    _logger.Add($"Container [{targetContainer.Id}] was deleted.");
                    Task.Delay(1000).Wait();
                    
                }
                catch (Exception ex)
                {
                    if (!HandleCosmosException(ex))
                        _logger.Add($"Unexpected error occured when deleteing container [{targetContainer.Id}].");

                }
            });
            
            return true;
        }

        private bool HandleCosmosException(Exception ex)
        {
            return (ex.InnerException != null && ex.InnerException.GetType() == typeof(CosmosException)
                    && (ex.InnerException as CosmosException).StatusCode == HttpStatusCode.NotFound);
        }

        //https://feedback.azure.com/forums/263030-azure-cosmos-db/suggestions/37441975--cosmosdb-nice-to-have-truncate-functionality-o
        // BestPractice is delete the container and recreate it 

        private void CreateContainers()
        {
            ConsoleHelper.UpdateConsole("Creating Containers....", _logger);
            List<Task<ContainerResponse>> createContainerTasks = new List<Task<ContainerResponse>>();

            Parallel.ForEach(_cosmosDBSettings.ContainerNamePrimaryKey, keyValuePair =>
            {
                
                Task<ContainerResponse> createTask = _database.CreateContainerAsync(keyValuePair.Key, $"/{keyValuePair.Value}");

                createContainerTasks.Add(createTask);
                
            });

            Task.WaitAll(createContainerTasks.ToArray());


            //TODO Check Status for each ContainerResponse 
            foreach (var createTask in createContainerTasks)
            {
                switch (createTask.Result.StatusCode)
                {
                    case HttpStatusCode.Created:
                    case HttpStatusCode.Accepted:
                        _logger.Add($"Container [{createTask.Result.Container.Id}] was created.");
                        break;
                    default:
                        _logger.Add($"Container [{createTask.Result.Container.Id}] was not created.");
                        break;
                }
            }

        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
