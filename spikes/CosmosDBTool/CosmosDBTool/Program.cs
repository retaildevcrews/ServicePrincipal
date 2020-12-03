using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace CosmosDBTool
{
    class Program
    {
        //Utility to reset CosmosDB Collections #271
        static void Main(string[] args)
        {
            string logFileName;
            Stopwatch stopWatch = new Stopwatch();
            using (CosmosDBSettings cosmosDBSettings = new CosmosDBSettings())
            {
                ConfirmationMessage(cosmosDBSettings);

                Console.WriteLine($"Initializing...");

                
                stopWatch.Start();

                using (var cosmosDBManager = new CosmosDBManager(cosmosDBSettings))
                {
                    cosmosDBManager.RecreateContainers();
                    logFileName = cosmosDBManager.LogFileName;
                }
                stopWatch.Stop();
            }
           

            File.AppendAllText(logFileName, $"{Environment.NewLine}***************  Time elapsed - {stopWatch.Elapsed}");

            Process.Start("notepad.exe", logFileName);
            Task.Delay(500).Wait();

            Console.Clear();
            Console.WriteLine($"{Environment.NewLine}Process completed!, time elapsed - {stopWatch.Elapsed}");
        }

        private static void ConfirmationMessage(CosmosDBSettings cosmosDBSettings)
        {
            string collectionNames = $"{Environment.NewLine}{Environment.NewLine}******* The following containers will be recreated [{string.Join(',',cosmosDBSettings.ContainerNames)}] *******{Environment.NewLine}";
            Console.WriteLine($"Your target Cosmos DB is [{cosmosDBSettings.DatabaseName}]{collectionNames}{Environment.NewLine}{Environment.NewLine}Enter 'Y' to continue?");

            try
            {
                string toContinue = Console.ReadLine();

                if (toContinue.Trim() != "Y")
                {
                    Console.WriteLine("Request was cancelled,  Goodbye!");
                    Environment.Exit(0);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Could not undestand your answer, please try again, Goodbye!");
                Environment.Exit(-1);
            }
        }
    }
}
