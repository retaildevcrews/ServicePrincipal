using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;


namespace CSE.Automation.TestsPrep
{
    class Program
    { 
        static void Main(string[] args)
        {
            bool interactive = false;

            if (args.Length == 1)
            {
                interactive = args[0].ToLower() == "interactive";
            }

            RunTestCasesRules(interactive);
        }


        private static void RunTestCasesRules(bool interactive = false)
        {
            Stopwatch stopWatch = new Stopwatch();

            if (interactive)
            {
                ConfirmationMessage();
                stopWatch.Start();
            }

            

            Environment.SetEnvironmentVariable("interactiveRun", interactive.ToString());// this will add this setting to config 

            using var configurationHelper = new ConfigurationHelper();

            string logFileName;
            using (TestCaseManager testCaseManager = new TestCaseManager(configurationHelper))
            {
                Console.WriteLine($"Starting process...");
                testCaseManager.Start();
                logFileName = testCaseManager.LogFileName;
            }

            
            if (interactive)
            {
                stopWatch.Stop();

                File.AppendAllText(logFileName, $"{Environment.NewLine}***************  Time elapsed - {stopWatch.Elapsed}");

                Process.Start("notepad.exe", logFileName);
                Task.Delay(500);

                Console.Clear();
                Console.WriteLine($"{Environment.NewLine}Process completed!, time elapsed - {stopWatch.Elapsed}");
            }

        }

        private static void ConfirmationMessage()
        {
            Console.WriteLine($"Please confirm. Enter 'Y' to continue?");

            try
            {
                string toContinue = Console.ReadLine();

                if (toContinue.Trim() != "Y")
                {
                    Console.WriteLine("Request was cancelled,  Goodbye!");
                    Environment.Exit(0);
                }

                Console.WriteLine($"Initializing...");
            }
            catch (Exception)
            {
                Console.WriteLine("Could not undestand your answer, please try again, Goodbye!");
                Environment.Exit(-1);
            }
        }
       
    }
}
