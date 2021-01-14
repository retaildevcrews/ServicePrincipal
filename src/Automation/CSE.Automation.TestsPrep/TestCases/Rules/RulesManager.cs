using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;

namespace CSE.Automation.TestsPrep.TestCases.Rules
{
    internal class RulesManager : IDisposable
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
            if (_availableServicePrincipals.Count < (_spSettings.TargetTestCaseList.Count * _spSettings.NumberOfSPObjectsToCreatePerTestCase))
            {
                throw new InvalidDataException($"The number of available SP objects in AAD do not match the number of 'SP per Ruleset Count'. Current Ruleset count is {RuleSetsList.Count} ");
            }

            Console.WriteLine($"Sorting AAD objects...");
            _availableServicePrincipals.Sort(new ServicePrincipalComparer());
            _availableUsers.Sort(new UserComparer());

            //NOTE:  switch back to regular foreach if you want to get "GetNext(x)" in sequence 
            //However it will increase execution time by 50%

            foreach (var ruleSetName in _spSettings.TargetTestCaseList)
            {
                string objectToInstantiate = $"CSE.Automation.TestsPrep.TestCases.Rules.{ruleSetName}, CSE.Automation.TestsPrep";

                var objectType = Type.GetType(objectToInstantiate);

                Console.WriteLine($"Executing Test Case {ruleSetName}...");

                var nextServicePrincipalSet = _availableServicePrincipals.GetNext(_spSettings.NumberOfSPObjectsToCreatePerTestCase);

                var nextUserSet = _availableUsers.GetNext(_spSettings.NumberOfSPObjectsToCreatePerTestCase);

                object[] args = { nextServicePrincipalSet, nextUserSet };

                var instantiatedObject = Activator.CreateInstance(objectType, args) as IRuleSet;

                RuleSetsList.Add(instantiatedObject);

                instantiatedObject.Execute();
            }

            Console.WriteLine($"Rules executed...");
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
        static object splock = new object();
        static object userlock = new object();

        public static List<ServicePrincipal> GetNext(this List<ServicePrincipal> availableSPs, int count)
        {
            lock (splock)
            {
                var result = availableSPs.Take(count).ToList();

                availableSPs.RemoveAll(x => result.Any(y => y.Id == x.Id));

                return result;
            }
        }

        public static List<User> GetNext(this List<User> availableUsers, int count)
        {
            lock (userlock)
            {
                var result = availableUsers.Take(count).ToList();

                availableUsers.RemoveAll(x => result.Any(y => y.Id == x.Id));

                return result;
            }
        }
    }
}
