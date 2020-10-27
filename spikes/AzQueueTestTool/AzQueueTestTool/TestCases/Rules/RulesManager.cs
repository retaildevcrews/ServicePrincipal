using AzQueueTestTool.TestCases.ServicePrincipals;
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
        public readonly List<IRuleSet> RuleSetsList = new List<IRuleSet>();
            
        private readonly List<ServicePrincipal> _availableServicePrincipals;

        private readonly ServicePrincipalSettings _spSettings;

        public RulesManager(List<ServicePrincipal> availableServicePrincipals, ServicePrincipalSettings spSettings)
        {
            _spSettings = spSettings;

            _availableServicePrincipals = availableServicePrincipals;
        }
        public void ExecuteAllRules()
        {
            if (_availableServicePrincipals.Count != (_spSettings.TargetTestCaseList.Count * _spSettings.NumberOfSPObjectsToCreatePerTestCase))
            {
                throw new InvalidDataException($"The number of available SP objects in AAD do not match the number of 'SP per Ruleset Count'. Current Ruleset count is {RuleSetsList.Count} ");
            }

            foreach (var ruleSetName in _spSettings.TargetTestCaseList)
            {
                string objectToInstantiate = $"AzQueueTestTool.TestCases.Rules.{ruleSetName}, AzQueueTestTool";

                var objectType = Type.GetType(objectToInstantiate);

                UpdateConsole($"Executing Test Case {ruleSetName}");

                var nextSpSet = _availableServicePrincipals.GetNext(_spSettings.NumberOfSPObjectsToCreatePerTestCase);

                object[] args = { nextSpSet };

                var instantiatedObject = Activator.CreateInstance(objectType, args) as IRuleSet;

                RuleSetsList.Add(instantiatedObject);

                instantiatedObject.Execute();


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
