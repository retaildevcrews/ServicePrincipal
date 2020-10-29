using System;

namespace CosmosDBTool
{
    class Program
    {
        //Utility to reset CosmosDB Collections #271
        static void Main(string[] args)
        {
            using (var cosmosDBManager = new CosmosDBManager())
            {
                cosmosDBManager.RecreateContainers();
            }
        }
    }
}
