using AzQueueTestTool.TestCases.Queues;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AzQueueTestTool.TestCases.ServicePrincipals
{
    internal class GraphHelper
    {
        private static GraphServiceClient _graphClient;
        private static List<User> _aADUsers;

        enum CollectionType
        {
            ServicePrincipal,
            Applications
        }

        public static void Initialize(IAuthenticationProvider authProvider)
        {
            _graphClient = new GraphServiceClient(authProvider);
        }

        private static async Task<List<User>> GetDirectoryObjects()
        {
            if (_aADUsers == null)
            {
                _aADUsers = new List<User>();

                var users = await _graphClient.Users
                            .Request()
                            .GetAsync();

                _aADUsers.AddRange(users.CurrentPage);
                //_aADUsers = users.
            }
 
            return _aADUsers;
        }

        private static async Task<List<User>> GetXRandomADUsersAsync(int userCount = 3)
        {
            List<User> users = await GetDirectoryObjects();

            Random random = new Random();

            List<User> result = new List<User>();



            for (int i = 1; i <= userCount; i++)
            {

                var user = users[random.Next(1, users.Count)];
                while (result.Contains(user))// make Owners list has unique values
                {
                    user = users[random.Next(1, users.Count)];
                }
                result.Add(user);
            }

            return result;
        }
      
        internal static async Task SetOwnersAsync(List<ServicePrincipal> targetServicePrincipals)
        {
            if (targetServicePrincipals != null && targetServicePrincipals.Count() > 0)
            {
                //GetAAD users once
                List<User> users = await GetXRandomADUsersAsync();

                var tasks = new List<Task>();

                foreach (var sp in targetServicePrincipals)
                {
                    Parallel.ForEach(users, user =>
                    {
                        var directoryObject = new DirectoryObject
                        {
                            Id = user.Id
                        };

                        Task thisTask = Task.Run(async () => await _graphClient.ServicePrincipals[$"{sp.Id}"].Owners.References
                        .Request()
                        .AddAsync(directoryObject));

                        tasks.Add(thisTask);
                    });
                }
                Task.WaitAll(tasks.ToArray());
            };
        }

        internal static void ClearOwners(List<ServicePrincipal> targetServicePrincipals)
        {
            if (targetServicePrincipals != null && targetServicePrincipals.Count() > 0)
            {
                var tasks = new List<Task>();
                foreach (var sp in targetServicePrincipals)
                {
                    // I need to Get Owners for each principal and then delete them  

                    Task<IServicePrincipalOwnersCollectionWithReferencesPage> taskOwners = _graphClient.ServicePrincipals[$"{sp.Id}"].Owners
                                                                    .Request()
                                                                    .GetAsync();

                    taskOwners.Wait();

                    List<string> ownerIdList = taskOwners.Result.CurrentPage.Select(x => x.Id).ToList();


                    if (ownerIdList != null && ownerIdList.Count > 0)
                    {
                        foreach(var ownerId in ownerIdList)
                        {
                            Task thisTask = Task.Run(async () => await _graphClient.ServicePrincipals[$"{sp.Id}"].Owners[$"{ownerId}"].Reference
                                                            .Request()
                                                            .DeleteAsync());

                            tasks.Add(thisTask);
                        }
                    }
                }
                Task.WaitAll(tasks.ToArray());
            }
        }
        
        internal static void ClearNotesField(List<ServicePrincipal> targetServicePrincipals)
        {
            if (targetServicePrincipals != null && targetServicePrincipals.Count() > 0)
            {
                var tasks = new List<Task>();

                Parallel.ForEach(targetServicePrincipals, async spObject =>
                {
                    var servicePrincipal = new ServicePrincipal
                    {
                        Notes = null
                    };

                    Task thisTask = Task.Run(() => _graphClient.ServicePrincipals[spObject.Id].Request().UpdateAsync(servicePrincipal));
                    tasks.Add(thisTask);
                });

                Task.WaitAll(tasks.ToArray());

            }
        }

        internal static void UpdateNotesFieldWithValidEmail(List<ServicePrincipal> targetServicePrincipals)
        {
            if (targetServicePrincipals != null && targetServicePrincipals.Count() > 0)
            {
                var tasks = new List<Task>();

                var validFormattedEmails = GetValidFormattedEmails(3);

                Parallel.ForEach(targetServicePrincipals, spObject =>
                {
                    var servicePrincipal = new ServicePrincipal
                    {
                        Notes = validFormattedEmails
                    };


                    Task thisTask = Task.Run(() => _graphClient.ServicePrincipals[spObject.Id].Request().UpdateAsync(servicePrincipal));
                    tasks.Add(thisTask);
                });

                Task.WaitAll(tasks.ToArray());

            }
        }

        private static string GetValidFormattedEmails(int emailCount)
        {
            const string testEmailBase = "TestEmail";
            string[] emailDomains = new string[]  { "@microsoft.com", "@outlook.com", "@gmail.com", "@amazon.com", "@yahoo.com" };
            List<string> emaiList = new List<string>();

            Random random = new Random();
 
            for (int i = 1; i <= emailCount; i++)
            {
                emaiList.Add($"{testEmailBase.AddRandomStringToEmail()}{emailDomains[random.Next(0, emailDomains.Length)]}" );
            }

            return string.Join(";", emaiList.Select(x => x));
        }

        internal static void UpdateNotesFieldWithAADOwnersEmail(List<ServicePrincipal> targetServicePrincipals)
        {
            if (targetServicePrincipals != null && targetServicePrincipals.Count() > 0)
            {
                var tasks = new List<Task>();

                //Parallel.ForEach(targetServicePrincipals, async spObject =>
                foreach(var spObject in targetServicePrincipals)
                {

                    Task< IServicePrincipalOwnersCollectionWithReferencesPage> taskOwners = _graphClient.ServicePrincipals[$"{spObject.Id}"].Owners
                                                                                        .Request()
                                                                                        .GetAsync();

                    taskOwners.Wait();

                    List<string> userPrincipalName = taskOwners.Result.CurrentPage.Where(x => (x as User).UserPrincipalName != null).Select(x => (x as User).UserPrincipalName).ToList();

                    var semicolonSeparatedOwnersEmail = string.Join(";", taskOwners.Result.CurrentPage.Select(x => (x as User).UserPrincipalName));

                    var servicePrincipal = new ServicePrincipal
                    {
                        Notes = semicolonSeparatedOwnersEmail
                    };


                    Task thisTask = Task.Run(() => _graphClient.ServicePrincipals[spObject.Id].Request().UpdateAsync(servicePrincipal));

                    tasks.Add(thisTask);

                    Task.Delay(500);
                };

                Task.WaitAll(tasks.ToArray());

            }
        }

        //https://stackoverflow.com/questions/56707404/microsoft-graph-only-returning-the-first-100-users
        internal static async Task<List<ServicePrincipal>> GetAllServicePrincipals(string spNamePefix, int count = 0)
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


        internal static async Task<List<Application>> GetAllApplicationAsync(string spNamePefix, int count = 0)
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


        internal static void CreateServicePrincipalAsync(string spNamePefix, int count, int lowerLimit = 1)
        {

            //var appRegistrationTasks = new List<Task<Application>>();
            var serviceTasks = new List<Task>();


            //Parallel.For(lowerLimit, count + 1, (i, state) =>
            for (int i = 1; i <= count; i++)
            {

                var application = new Application
                {
                    DisplayName = $"{spNamePefix}-{i}"
                };

                Task<Application> appTask = Task.Run(() => _graphClient.Applications.Request().AddAsync(application));
                appTask.Wait();

                var servicePrincipal = new ServicePrincipal
                {
                    AppId = appTask.Result.AppId
                };


                Task spTask = Task.Run(() => _graphClient.ServicePrincipals.Request().AddAsync(servicePrincipal));
                serviceTasks.Add(spTask);


            };

            Task.WaitAll(serviceTasks.ToArray());

            Console.WriteLine("app registration and service principal creation done, press a key to continue");

        }

   
        private static void AddServicePrincipals(string spNamePefix)//IList<Application> applicationsList)
        {

            var applicationsList = GetAllApplicationAsync(spNamePefix).Result;
            var servicePrinciaplTasks = new List<Task>();

            //Parallel.ForEach(applicationsList, app =>
            foreach(var app in applicationsList)
            {
                var servicePrincipal = new ServicePrincipal
                {
                    AppId = app.AppId
                };
                

                Task thisTask = Task.Run(() => _graphClient.ServicePrincipals.Request().AddAsync(servicePrincipal));

                servicePrinciaplTasks.Add(thisTask);
                
                Task.Delay(500);
            }

          

            Task.WaitAll(servicePrinciaplTasks.ToArray());

        }

        internal static async Task GetUsersAsync()
        {
            var selectFields = new[] { "appId", "displayName", "notes", "owners", "notificationEmailAddresses" };
            var users = await _graphClient.Users
                .Request()
                //.Filter("identities/any(c:c/issuerAssignedId eq 'j.smith@yahoo.com' and c/issuer eq 'contoso.onmicrosoft.com')")
                .Select(string.Join(',', selectFields))
                .GetAsync();

        }

        internal static void DeleteServicePrincipalsAsync(IList<ServicePrincipal> servicePrincipalList)
        {

            if (servicePrincipalList != null && servicePrincipalList.Count() > 0)
            {
                var tasks = new List<Task>();

                foreach(var spObject in servicePrincipalList)
                {

                    Task thisTask = Task.Run(() => _graphClient.ServicePrincipals[spObject.Id].Request().DeleteAsync());
                    tasks.Add(thisTask);
                    Task.Delay(500);
                }

                Task.WaitAll(tasks.ToArray());
            }


            Console.WriteLine("DeleteServicePrincipalsAsync done");

        }

        internal static void DeleteRegisteredApplicationsAsync(IList<Application> applicationsList)
        {

            if (applicationsList != null && applicationsList.Count() > 0)
            {
                var tasks = new List<Task>();

                foreach(var appObject in applicationsList)
                {

                    Task thisTask = Task.Run(() => _graphClient.Applications[appObject.Id].Request().DeleteAsync());
                    tasks.Add(thisTask);
                    Task.Delay(500);
                }

                Task.WaitAll(tasks.ToArray());
            }


            Console.WriteLine("DeleteRegisteredApplicationsAsync done");
            

        }

        internal static void UpdateServicePrincipalNote(string servicePrincipalNote, IList<ServicePrincipal> servicePrincipalList)
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

        internal static async Task<IList<ServicePrincipal>> GetAllServicePrincipalsWithNotes(string spNamePefix, int count = 0)
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
