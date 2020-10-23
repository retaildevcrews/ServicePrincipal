using AzQueueTestTool.TestCases;
using AzQueueTestTool.TestCases.Queues;
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
        //Create test utility to inject messages directly into the 'evaluate' queue and 'update' queue.

        static void Main(string[] args)
        {
            GenerateMessagesForTesting();
        }

        private static void GenerateMessagesForTesting()
        {
            using (var queueSettings = new QueueSettings())
            {

                ConfirmationMessage(queueSettings);

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                using (TestCaseManager testCaseManager = new TestCaseManager(queueSettings))
                {
                    testCaseManager.Start();
                }

                stopWatch.Stop();
                Console.Clear();
                //Console.WriteLine(sb.ToString());
                Console.WriteLine($"{Environment.NewLine}Process completed!, time elapsed - {stopWatch.Elapsed}");
            }
        }

        private static void InsertMessages()
        {
            using (var queueSettings = new QueueSettings())
            {

                ShouldProceed(queueSettings);

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                StringBuilder sb = new StringBuilder();
                Parallel.ForEach(queueSettings.QueueNamesList, (queueName) =>
                {
                    using (var queueManager = new QueueManager(queueName, queueSettings.MessageBase, queueSettings.StorageConnectionString))
                    {
                        queueManager.AddBaseMessages(queueSettings.MessageCount);
                        sb.AppendLine(queueManager.StatusMessage);
                    }
                });

                stopWatch.Stop();
                Console.Clear();
                Console.WriteLine(sb.ToString());
                Console.WriteLine($"{Environment.NewLine}Process completed!, time elapsed - {stopWatch.Elapsed}");
            }
        }
        private static void ShouldProceed(QueueSettings queueSettings)
        {
            string accountName = GetAccountName(queueSettings.StorageConnectionString);

            Console.WriteLine($"Your target Storage Account is [{accountName}] and [{queueSettings.MessageCount}] messages will be pushed to each Queue [{queueSettings.QueueNames}]{Environment.NewLine}{Environment.NewLine}Enter 'Y' to continue?");

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

        private static void ConfirmationMessage(QueueSettings queueSettings)
        {
            string accountName = GetAccountName(queueSettings.StorageConnectionString);

            Console.WriteLine($"Your target Storage Account is [{accountName}] and [{queueSettings.MessageCount}] messages will be pushed to Queue [Evaluate]{Environment.NewLine}{Environment.NewLine}Enter 'Y' to continue?");

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
