using AzQueueTestTool.TestCases.ServicePrincipals;
using CSE.Automation.Model;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace AzQueueTestTool.TestCases.Rules
{
    public class RulesManager : IDisposable
    {
        public readonly List<IRuleSet> RuleSetsList = new List<IRuleSet>();
            
        private readonly List<ServicePrincipal> _availableServicePrincipals;

        private readonly List<User> _availableUsers;

        private readonly ServicePrincipalSettings _spSettings;

        public RulesManager(List<ServicePrincipal> availableServicePrincipals, List<User> availableUsers, ServicePrincipalSettings spSettings)
        {
            _spSettings = spSettings;

            _availableServicePrincipals = availableServicePrincipals;
            _availableUsers = availableUsers;
        }


        public void ExecuteAllRules()
        {
            if (_availableServicePrincipals.Count != (_spSettings.TargetTestCaseList.Count * _spSettings.NumberOfSPObjectsToCreatePerTestCase))
            {
                throw new InvalidDataException($"The number of available SP objects in AAD do not match the number of 'SP per Ruleset Count'. Current Ruleset count is {RuleSetsList.Count} ");
            }

            UpdateConsole($"Sorting AAD objects...");
            _availableServicePrincipals.Sort(new ServicePrincipalComparer());
            _availableUsers.Sort(new UserComparer());

            foreach (var ruleSetName in _spSettings.TargetTestCaseList)
            {
                string objectToInstantiate = $"AzQueueTestTool.TestCases.Rules.{ruleSetName}, AzQueueTestTool";

                var objectType = Type.GetType(objectToInstantiate);

                UpdateConsole($"Executing Test Case {ruleSetName}...");

                var nextServicePrincipalSet = _availableServicePrincipals.GetNext(_spSettings.NumberOfSPObjectsToCreatePerTestCase);

                var nextUserSet = _availableUsers.GetNext(_spSettings.NumberOfSPObjectsToCreatePerTestCase);

                object[] args = { nextServicePrincipalSet, nextUserSet  };

                var instantiatedObject = Activator.CreateInstance(objectType, args ) as IRuleSet;

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

    class ServicePrincipalComparer : IComparer<ServicePrincipal>
    {
        public int Compare(ServicePrincipal sp1, ServicePrincipal sp2)
        {
            var index1 = int.Parse(sp1.DisplayName.Split('-').ToList().Last());
            var index2 = int.Parse(sp2.DisplayName.Split('-').ToList().Last());
            return index1.CompareTo(index2);
        }
    }

    class UserComparer : IComparer<User>
    {
        public int Compare(User user1, User user2)
        {
            var index1 = int.Parse(user1.DisplayName.Split('-').ToList().Last());
            var index2 = int.Parse(user2.DisplayName.Split('-').ToList().Last());
            return index1.CompareTo(index2);
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

        public static List<User> GetNext(this List<User> availableUsers, int count)
        {
            var result = availableUsers.Take(count).ToList();

            availableUsers.RemoveAll(x => result.Any(y => y.Id == x.Id));

            return result;
        }
    }
}
