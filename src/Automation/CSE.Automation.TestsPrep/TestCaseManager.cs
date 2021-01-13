using CSE.Automation.TestsPrep.TestCases.Rules;
using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSE.Automation.TestsPrep
{
    internal class TestCaseManager : IDisposable
    {
        private readonly ServicePrincipalSettings _spSettings;

        public string LogFileName { get; private set; }

        public TestCaseManager(ConfigurationHelper configHelper)
        {
            _spSettings = new ServicePrincipalSettings(configHelper);
        }

        internal void Start()
        {
            List<Task> queryObjects = new List<Task>();
          
            Task<List<ServicePrincipal>> getSPsTask = Task.Run(() => GetTargetServicePrincipals());
            queryObjects.Add(getSPsTask);

            Task<List<User>> getUserTask = Task.Run(() => GetTargetAADUsers());
            queryObjects.Add(getUserTask);

            Task.WaitAll(queryObjects.ToArray());

            ExecuteAllRules(getSPsTask.Result, getUserTask.Result);
        }

        internal void Cleanup()
        {
            using (ServicePrincipalManager ServicePrincipalManager = new ServicePrincipalManager(_spSettings))
            {
                ServicePrincipalManager.DeleteServicePrincipals();
            }
        }

        private void ExecuteAllRules(List<ServicePrincipal> availableServicePrincipals, List<User> availableUsers)
        {
            using (RulesManager rulesManager = new RulesManager(availableServicePrincipals, availableUsers, _spSettings))
            {
                rulesManager.ExecuteAllRules();

                GenerateLog(rulesManager.RuleSetsList);
            }
        }


        private void GenerateLog(List<IRuleSet> ruleSetsList)
        {
            if (_spSettings.InteractiveRun)
            {
                List<string> logger = new List<string>();

                foreach (var ruleSet in ruleSetsList)
                {
                    if (ruleSet.ServicePrincipals == null || ruleSet.ServicePrincipals.Count == 0)
                        continue;

                    StringBuilder sbLog = new StringBuilder();

                    BuildLogEntry(ruleSet, sbLog);

                    logger.Add(sbLog.ToString());

                    ConsoleHelper.UpdateConsole($"Sending [{ruleSet.GetType().Name}] messages to Queue...", _spSettings.InteractiveRun);

                }

                System.IO.Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"));

                LogFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", DateTime.Now.ToString("yyyyMMddHHmmss") + "_Execution.log");
                System.IO.File.WriteAllLines(LogFileName, logger);
                Task.Delay(500);
            }
        }

        private void BuildLogEntry(IRuleSet ruleSet, StringBuilder sbLog)
        {
            sbLog.AppendLine($"*** {ruleSet.GetType().Name} ***");
            sbLog.AppendLine($"-> ServicePrincipals{Environment.NewLine}{string.Join(Environment.NewLine, ruleSet.ServicePrincipals.Select(x => new { name = x.DisplayName, x.Id }).ToList())}");

            if (ruleSet.HasOwners)
                sbLog.AppendLine($"-> Owners{Environment.NewLine}{string.Join(Environment.NewLine, ruleSet.AADUsers.Select(x => new { name = x.DisplayName, x.Id }).ToList())}");
            else
                sbLog.AppendLine($"-> Owners{Environment.NewLine} <<none>>");
        }


        private List<ServicePrincipal> GetTargetServicePrincipals()
        {
            using (ServicePrincipalManager ServicePrincipalManager = new ServicePrincipalManager(_spSettings))
            {
                ConsoleHelper.UpdateConsole($"Getting Service Principal Objects...", _spSettings.InteractiveRun);
                
                return ServicePrincipalManager.GetOrCreateServicePrincipals();
            }

        }

        private List<User> GetTargetAADUsers()
        {
            using (ServicePrincipalManager ServicePrincipalManager = new ServicePrincipalManager(_spSettings))
            {
                ConsoleHelper.UpdateConsole($"Getting User Objects...", _spSettings.InteractiveRun);
                return ServicePrincipalManager.GetOrCreateUsers();
            }

        }

        internal void DeleteServicePrincipals()
        {
            using (ServicePrincipalManager ServicePrincipalManager = new ServicePrincipalManager(_spSettings))
            {
                ServicePrincipalManager.DeleteServicePrincipals();
            }
        }

       
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
