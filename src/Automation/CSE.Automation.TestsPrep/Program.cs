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
            RunTestCasesRules();
        }


        private static void RunTestCasesRules()
        {
            Stopwatch stopWatch = new Stopwatch();
            string logFileName;
            try
            {
                stopWatch.Start();

                using var configurationHelper = new ConfigurationHelper();

                using (TestCaseManager testCaseManager = new TestCaseManager(configurationHelper))
                {
                    Console.WriteLine($"Starting process...");
                    testCaseManager.Start();
                    logFileName = testCaseManager.LogFileName;

                    Console.WriteLine($"{Environment.NewLine}--------------  Log  --------------{Environment.NewLine}");
                    Console.WriteLine(testCaseManager.GetExecutionLog());
                }
            }
            finally
            {
                stopWatch.Stop();
            }

            File.AppendAllText(logFileName, $"{Environment.NewLine}***************  Time elapsed - {stopWatch.Elapsed}");
            Console.WriteLine($"{Environment.NewLine}Process completed!, time elapsed - {stopWatch.Elapsed}");

            Console.WriteLine($"Log file at: {logFileName}");
        }
    }
}
