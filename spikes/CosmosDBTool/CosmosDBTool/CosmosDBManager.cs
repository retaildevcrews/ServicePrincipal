using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDBTool
{
    class CosmosDBManager : IDisposable
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Database _database;
        private List<Container> _containerList = new List<Container>();
        private CosmosDBSettings _cosmosDBSettings = new CosmosDBSettings();
        private StringBuilder _sbError = new StringBuilder();
        public CosmosDBManager()
        {
            _cosmosDBSettings = new CosmosDBSettings();
            _cosmosClient = new CosmosClient(_cosmosDBSettings.ConnectionString);
            _database = _cosmosClient.GetDatabase(_cosmosDBSettings.DatabaseName);

            InitializeContainers();
        }

        private void InitializeContainers()
        {
           foreach(var colInfo in _cosmosDBSettings.ContainerNamePrimaryKey)
            {  
                //Container proxy reference doesn't guarantee existence.
                var container = _cosmosClient.GetContainer(_cosmosDBSettings.DatabaseName, colInfo.Key);
                if (container != null)
                {
                    //Save Conteiner Properties such container Throughput to be reuse 

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
            if (_sbError.Length > 0)
            {
                Console.WriteLine(_sbError.ToString()); // need to save it to a file
            }
        }

        private bool DeleteContainers()
        {
            //List<Task<ContainerResponse>> deleteContainerTasks = new List<Task<ContainerResponse>>();

            foreach (var targetContainer in _containerList) 
            //Parallel.ForEach(_containerList, targetContainer =>
            {
                //Container proxy reference doesn't guarantee existence. soif container does not exist it will throw an exception.
                Task<ContainerResponse> deleteTask = Task.Run(() =>targetContainer.DeleteContainerAsync());
                try
                {
                    deleteTask.Wait();
                }
                catch (Exception ex)
                {
                    if (!HandleCosmosException(ex))
                        _sbError.AppendLine($"Unexpected error occured when deleteing container [{targetContainer.Id}].");
                }
                //deleteContainerTasks.Add(deleteTask);
            }

            //Task.WaitAll(deleteContainerTasks.ToArray());

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
            List<Task<ContainerResponse>> createContainerTasks = new List<Task<ContainerResponse>>();
            foreach(var keyValuePair in _cosmosDBSettings.ContainerNamePrimaryKey)
            //Parallel.ForEach(_cosmosDBSettings.ContainerNamePrimaryKey, keyValuePair =>
            {

                Task<ContainerResponse> createTask = Task.Run(() => _database.CreateContainerIfNotExistsAsync(keyValuePair.Key, $"/{keyValuePair.Value}"));
                createTask.Wait();
                
                //createContainerTasks.Add(createTask);
            }

            //Task.WaitAll(createContainerTasks.ToArray());

            //TODO Check Status for each ContainerResponse 
            foreach (var createTask in createContainerTasks)
            {
                switch (createTask.Result.StatusCode)
                {
                    case HttpStatusCode.Created:
                    case HttpStatusCode.Accepted: break;
                    default:
                        _sbError.AppendLine($"Container [{createTask.Result.Container.Id}] was not created.");
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
