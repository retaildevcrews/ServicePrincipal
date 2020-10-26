using CSE.Automation.Model;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AzQueueTestTool.TestCases.Rules
{
    public class RulesManager : IDisposable
    {
        public enum CaseId
        {
            TC1,
            TC2,
            TC3,
            TC4,
            TC5,
            TC6,
            TC7,
            TC8,
            TC9,
            TC10,
            TC11
        }
        
        public readonly List<IRuleSet> RuleSetsList = new List<IRuleSet> { new RuleSet1(), new RuleSet2(), new RuleSet3(), new RuleSet4(), 
                                                            new RuleSet5(), new RuleSet6(), new RuleSet7(), new RuleSet8(), new RuleSet9(), 
                                                            new RuleSet10(), new RuleSet11()};

        private readonly int _servicePrincipalObjectsPerRuleSet;
        private readonly List<ServicePrincipal> _availableServicePrincipals;
        public RulesManager(List<ServicePrincipal> availableServicePrincipals, int servicePrincipalObjectsPerRuleSet)
        {
            _servicePrincipalObjectsPerRuleSet = servicePrincipalObjectsPerRuleSet;

            _availableServicePrincipals = availableServicePrincipals;
        }
        public void ExecuteAllRules(List<string> targetTestCaseList)
        {
            if (_availableServicePrincipals.Count != (RuleSetsList.Count * _servicePrincipalObjectsPerRuleSet))
            {
                throw new InvalidDataException($"The number of available SP objects in AAD do not match the number of 'SP per Ruleset Count'. Current Ruleset count is {RuleSetsList.Count} ");
            }

            foreach (var ruleSet in RuleSetsList)// Parrallel loop?
            {
                if (targetTestCaseList.Contains(ruleSet.TestCaseId.ToString()))// execute only Test case defined in configuration
                {
                    UpdateConsole($"Executing Test Case {ruleSet.TestCaseId.ToString()}");

                    var nextSpSet = _availableServicePrincipals.GetNext(_servicePrincipalObjectsPerRuleSet);
                    ruleSet.Execute(nextSpSet);

                }
            }
        }

        public void UpdateConsole(string message)
        {
            Console.Write(string.Format("\r{0}", "".PadLeft(Console.CursorLeft, ' ')));
            Console.Write(string.Format("\r{0}", message));

        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }

    public static class RuleSetExtensions
    {

        public static List<ServicePrincipal> GetNext(this List<ServicePrincipal> availableSPs, int count)
        {
            var result = availableSPs.Take(count).ToList();

            availableSPs.RemoveAll(x => result.Any(y => y.Id == x.Id));

            return result;
        }
    }
}
