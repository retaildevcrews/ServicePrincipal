using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AzQueueTestTool.TestCases.ServicePrincipals
{
    class GraphHelper
    {
        private static GraphServiceClient _graphClient;
       
        enum CollectionType
        {
            ServicePrincipal,
            Applications
        }

        public static void Initialize(IAuthenticationProvider authProvider)
        {
            _graphClient = new GraphServiceClient(authProvider);
        }

        //https://stackoverflow.com/questions/56707404/microsoft-graph-only-returning-the-first-100-users
        public static async Task<List<ServicePrincipal>> GetAllServicePrincipalsAsync(string spNamePefix, int count= 0)
        {
            try
            {
                List<ServicePrincipal> servicePrincipalList = new List<ServicePrincipal>();


                var servicePrincipalsPage = await _graphClient.ServicePrincipals
               .Request()
               .Filter($"startswith(displayName,'{spNamePefix}')")
               .GetAsync();


                //https://stackoverflow.com/questions/56707404/microsoft-graph-only-returning-the-first-100-users

                servicePrincipalList.AddRange(servicePrincipalsPage.CurrentPage);

                bool breakOnListCountGreaterOrEqualsToCount = (count > 0);

                while (servicePrincipalsPage.NextPageRequest != null )
                {
                    if (breakOnListCountGreaterOrEqualsToCount && servicePrincipalList.Count >= count)
                    {
                        break;
                    }
                    servicePrincipalsPage = await servicePrincipalsPage.NextPageRequest.GetAsync();
                    servicePrincipalList.AddRange(servicePrincipalsPage.CurrentPage);
                }

                if (count > 0)
                    return servicePrincipalList.Take(count).ToList();
                else
                   return servicePrincipalList.ToList();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting All Owners: {ex.Message}");
                return null;
            }
        }

        internal static void ClearOwners(List<ServicePrincipal> targetServicePrincipals)
        {
            throw new NotImplementedException();
        }

        internal static void ClearNotesFiled(List<ServicePrincipal> targetServicePrincipals)
        {
            throw new NotImplementedException();
        }

        internal static void UpdateNotesFieldWithValidEmail(List<ServicePrincipal> targetServicePrincipals)
        {
            throw new NotImplementedException();
        }

        internal static void UpdateNotesFieldWithAADOwnersEmail(List<ServicePrincipal> targetServicePrincipals)
        {
            throw new NotImplementedException();
        }

        internal static void SetOwners(List<ServicePrincipal> targetServicePrincipals)
        {
            throw new NotImplementedException();
        }

        public static async Task<List<Application>> GetAllApplicationAsync(string spNamePefix, int count = 0)
        {
            try
            {
                List<Application> applicationList = new List<Application>();


                var applicationsPage = await _graphClient.Applications
               .Request()
               .Filter($"startswith(displayName,'{spNamePefix}')")
               .GetAsync();


                //https://stackoverflow.com/questions/56707404/microsoft-graph-only-returning-the-first-100-users

                applicationList.AddRange(applicationsPage.CurrentPage);

                bool breakOnListCountGreaterOrEqualsToCount = (count > 0);

                while (applicationsPage.NextPageRequest != null)
                {
                    if (breakOnListCountGreaterOrEqualsToCount && applicationList.Count >= count)
                    {
                        break;
                    }

                    applicationsPage = await applicationsPage.NextPageRequest.GetAsync();
                    applicationList.AddRange(applicationsPage.CurrentPage);
                }

                if (count > 0)
                    return applicationList.Take(count).ToList();
                else
                    return applicationList.ToList();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting All Owners: {ex.Message}");
                return null;
            }
        }


        public static void CreateServicePrincipalAsync(string spNamePefix, int count, int lowerLimit = 1)
        {

            //IList<Application> applicationList = new List<Application>();

            var appRegistrationTasks = new List<Task<Application>>();


            Parallel.For(lowerLimit, count + 1, (i, state) =>
            //for (int i = 1; i<=count; i++)
            {

                var application = new Application
                {
                    DisplayName = $"{spNamePefix}-{i}"
                };

                appRegistrationTasks.Add(Task.Run(() => _graphClient.Applications.Request().AddAsync(application)));


            });

            Task.WaitAll(appRegistrationTasks.ToArray()); // Exception thrpwn here 


            AddServicePrincipals(appRegistrationTasks);

            Console.WriteLine("app registration done, press a key to continue");
            
        }

   
        private static void AddServicePrincipals(List<Task<Application>> applicationsList)//IList<Application> applicationsList)
        {

            var servicePrinciaplTasks = new List<Task>();

            Parallel.ForEach(applicationsList, app =>
            {
                var servicePrincipal = new ServicePrincipal
                {
                    AppId = app.Result.AppId
                };
                

                Task thisTask = Task.Run(() => _graphClient.ServicePrincipals.Request().AddAsync(servicePrincipal));

                servicePrinciaplTasks.Add(thisTask);
            });

          

            Task.WaitAll(servicePrinciaplTasks.ToArray());

        }

        public static async Task GetUsersAsync()
        {
            var selectFields = new[] { "appId", "displayName", "notes", "owners", "notificationEmailAddresses" };
            var users = await _graphClient.Users
                .Request()
                //.Filter("identities/any(c:c/issuerAssignedId eq 'j.smith@yahoo.com' and c/issuer eq 'contoso.onmicrosoft.com')")
                .Select(string.Join(',', selectFields))
                .GetAsync();

        }

        public static void DeleteServicePrincipalsAsync(IList<ServicePrincipal> servicePrincipalList)
        {

            if (servicePrincipalList != null && servicePrincipalList.Count() > 0)
            {
                var tasks = new List<Task>();

                Parallel.ForEach(servicePrincipalList, spObject =>
                {

                    Task thisTask = Task.Run(() => _graphClient.ServicePrincipals[spObject.Id].Request().DeleteAsync());
                    tasks.Add(thisTask);
                });

                Task.WaitAll(tasks.ToArray());
            }


            Console.WriteLine("DeleteServicePrincipalsAsync done");

        }

        public static void DeleteRegisteredApplicationsAsync(IList<Application> applicationsList)
        {

            if (applicationsList != null && applicationsList.Count() > 0)
            {
                var tasks = new List<Task>();

                Parallel.ForEach(applicationsList, appObject =>
                {

                    Task thisTask = Task.Run(() => _graphClient.Applications[appObject.Id].Request().DeleteAsync());
                    tasks.Add(thisTask);
                });

                Task.WaitAll(tasks.ToArray());
            }


            Console.WriteLine("DeleteRegisteredApplicationsAsync done");
            

        }

        public static void UpdateServicePrincipalNote(string servicePrincipalNote, IList<ServicePrincipal> servicePrincipalList)
        {

            if (servicePrincipalList != null && servicePrincipalList.Count() > 0)
            {
                var tasks = new List<Task>();

                Parallel.ForEach(servicePrincipalList, spObject =>
                {

                    var servicePrincipal = new ServicePrincipal
                    {
                        Notes = $"{servicePrincipalNote} - {spObject.Id}"
                    };

                    Task thisTask = Task.Run(() => _graphClient.ServicePrincipals[spObject.Id].Request().UpdateAsync(servicePrincipal));
                    tasks.Add(thisTask);
                });

                Task.WaitAll(tasks.ToArray());

            }
        }


        public static async Task<IList<ServicePrincipal>> GetAllServicePrincipalsWithNotes(string spNamePefix, int count = 0)
        {
            try
            {
                List<ServicePrincipal> servicePrincipalList = new List<ServicePrincipal>();


                var servicePrincipalsPage = await _graphClient.ServicePrincipals
               .Request()
               .Filter($"startswith(displayName,'{spNamePefix}')")
               .GetAsync();


                //https://stackoverflow.com/questions/56707404/microsoft-graph-only-returning-the-first-100-users

                servicePrincipalList.AddRange(servicePrincipalsPage.CurrentPage);

                bool breakOnListCountGreaterOrEqualsToCount = (count > 0);

                while (servicePrincipalsPage.NextPageRequest != null)
                {
                    if (breakOnListCountGreaterOrEqualsToCount && servicePrincipalList.Count >= count)
                    {
                        break;
                    }
                    servicePrincipalsPage = await servicePrincipalsPage.NextPageRequest.GetAsync();
                    servicePrincipalList.AddRange(servicePrincipalsPage.CurrentPage);
                }

                if (count > 0)
                    return servicePrincipalList.Where(x => !string.IsNullOrEmpty(x.Notes)).Take(count).ToList();
                else
                    return servicePrincipalList.Where(x => !string.IsNullOrEmpty(x.Notes)).ToList();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting All Owners: {ex.Message}");
                return null;
            }
        }
    }
}
