using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServicePrincipalNotesUpdater
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

        public static async void CreateUpdateServicePrincipalNote(string servicePrincipalId, string servicePrincipalNote)
        {
            var servicePrincipal = new ServicePrincipal
            {
                Notes = servicePrincipalNote
            };

            try
            {
                await _graphClient.ServicePrincipals[servicePrincipalId].Request().UpdateAsync(servicePrincipal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Updating Notes for Service Principal Id: {servicePrincipalId}  : {ex.Message}");
                return;
            }

           

        }

        //https://stackoverflow.com/questions/56707404/microsoft-graph-only-returning-the-first-100-users
        public static async Task<IList<ServicePrincipal>> GetAllServicePrincipalsAsync(string spNamePefix, int count= 0)
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


                while (servicePrincipalsPage.NextPageRequest != null)
                {
                    servicePrincipalsPage = await servicePrincipalsPage.NextPageRequest.GetAsync();
                    servicePrincipalList.AddRange(servicePrincipalsPage.CurrentPage);
                }

                if (count > 0)
                    //return servicePrincipalList.Where(x => x.DisplayName.StartsWith(spNamePefix)).Take(count).ToList();
                    return servicePrincipalList.Take(count).ToList();
                else
                   //return servicePrincipalList.Where(x => x.DisplayName.StartsWith(spNamePefix)).ToList();
                   return servicePrincipalList.ToList();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting All Owners: {ex.Message}");
                return null;
            }
        }

        public static async Task<IList<Application>> GetAllApplicationAsync(string spNamePefix, int count = 0)
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


                while (applicationsPage.NextPageRequest != null)
                {
                    applicationsPage = await applicationsPage.NextPageRequest.GetAsync();
                    applicationList.AddRange(applicationsPage.CurrentPage);
                }

                if (count > 0)
                    //return servicePrincipalList.Where(x => x.DisplayName.StartsWith(spNamePefix)).Take(count).ToList();
                    return applicationList.Take(count).ToList();
                else
                    //return servicePrincipalList.Where(x => x.DisplayName.StartsWith(spNamePefix)).ToList();
                    return applicationList.ToList();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting All Owners: {ex.Message}");
                return null;
            }
        }


       public static async void CreateServicePrincipalAsync(string spNamePefix, int count)
        {
            
            //IList<Application> applicationList = new List<Application>();

            var appRegistrationTasks = new List<Task<Application>>();


            //Parallel.For(1, count + 1, (i, state) =>
            for (int i = 1; i<=count; i++)
            {

                var application = new Application
                {
                    DisplayName = $"{spNamePefix}-{i}"
                };

                appRegistrationTasks.Add(Task.Run(() => _graphClient.Applications.Request().AddAsync(application)));
                

            }

            Task.WaitAll(appRegistrationTasks.ToArray()); // Exception thrpwn here 

            
            //var applicationsList = GetAllApplicationAsync(spNamePefix).Result;//Test

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

    }
}
