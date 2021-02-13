using System;
using System.Threading;
using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.Helpers
{
    internal class ServicePrincipalHelper : IDisposable
    {
        internal void DeleteServicePrincipal(string servicePrincipalToDelete)
        {
            var servicePrincipalList = GraphHelper.GetAllServicePrincipals(servicePrincipalToDelete).Result;

            if (servicePrincipalList.Count > 0)
            {
                GraphHelper.DeleteServicePrincipalsAsync(servicePrincipalList);

                while (servicePrincipalList.Count > 0)
                {
                    // We need to make sure Object was removed 
                    Thread.Sleep(1000);
                    servicePrincipalList = GraphHelper.GetAllServicePrincipals(servicePrincipalToDelete).Result;
                }
            }


            var applicationsList = GraphHelper.GetAllApplicationAsync(servicePrincipalToDelete).Result;

            if (applicationsList.Count > 0)
            {
                GraphHelper.DeleteRegisteredApplicationsAsync(applicationsList);

                while (applicationsList.Count > 0)
                {
                    // We need to make sure Object was removed 
                    Thread.Sleep(1000);
                    applicationsList = GraphHelper.GetAllApplicationAsync(servicePrincipalToDelete).Result;
                }
            }
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
