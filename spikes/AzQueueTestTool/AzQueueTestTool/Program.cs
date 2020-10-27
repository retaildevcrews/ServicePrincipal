using AzQueueTestTool.TestCases;
using AzQueueTestTool.TestCases.Queues;
using AzQueueTestTool.TestCases.ServicePrincipals;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzQueueTestTool
{
    class Program
    {
        static void Main(string[] args)
        {
            GenerateMessagesForTestCases();
        }


        private static void GenerateMessagesForTestCases()
        {
            using ( var queueSettings = new QueueSettings())
            {

                ConfirmationMessage(queueSettings);

                Console.WriteLine($"Initializing...");

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                using (TestCaseManager testCaseManager = new TestCaseManager(queueSettings))
                {
                    Console.WriteLine($"Starting process...");
                    testCaseManager.Start();
                }

                stopWatch.Stop();
                Console.Clear();
                Console.WriteLine($"{Environment.NewLine}Process completed!, time elapsed - {stopWatch.Elapsed}");
            }
        }

        private static void ConfirmationMessage(QueueSettings queueSettings)
        {
            string accountName = GetAccountName(queueSettings.StorageConnectionString);

            Console.WriteLine($"Your target Storage Account is [{accountName}] and messages will be pushed to Queue [Evaluate]{Environment.NewLine}{Environment.NewLine}Enter 'Y' to continue?");

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

        private static string GetAccountName(string storageConnectionString)
        {
            List<string> storageConnectionStringPartsList = storageConnectionString.Split(';').ToList();

            List<string> itemPartsList = storageConnectionStringPartsList.FirstOrDefault(x => x.StartsWith("AccountName")).Split('=').ToList();

            return itemPartsList[1];

        }
    }
}
