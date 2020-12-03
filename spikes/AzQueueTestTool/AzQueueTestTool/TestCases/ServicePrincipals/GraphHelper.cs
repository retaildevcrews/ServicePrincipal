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

        private static EmailSettings _emailSettings;

        public static void Initialize(IAuthenticationProvider authProvider)
        {
            _graphClient = new GraphServiceClient(authProvider);
            _emailSettings = new EmailSettings();
        }

        internal static bool SetOwners(List<ServicePrincipal> targetServicePrincipals, List<User> targetUsers)
        {
            if (targetServicePrincipals != null && targetServicePrincipals.Count() > 0)
            {
                var tasks = new List<Task>();

                foreach (var sp in targetServicePrincipals)
                {
                    Parallel.ForEach(targetUsers, user =>
                    {
                        var directoryObject = new DirectoryObject
                        {
                            Id = user.Id
                        };

                        Task thisTask = _graphClient.ServicePrincipals[$"{sp.Id}"].Owners.References
                                        .Request()
                                        .AddAsync(directoryObject);

                        tasks.Add(thisTask);
                    });
                }
                Task.WaitAll(tasks.ToArray());
            };
            return true;
        }

        internal static bool ClearOwners(List<ServicePrincipal> targetServicePrincipals)
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
                            Task thisTask = _graphClient.ServicePrincipals[$"{sp.Id}"].Owners[$"{ownerId}"].Reference
                                                            .Request()
                                                            .DeleteAsync();

                            tasks.Add(thisTask);
                        }
                    }
                }
                Task.WaitAll(tasks.ToArray());
            }
            return true;
        }

        internal static Dictionary<string,string> GetOwnersDisplayNameAndUserPrincipalNameKeyValuePair(ServicePrincipal spObject)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (spObject != null)
            {
                Task<IServicePrincipalOwnersCollectionWithReferencesPage> taskOwners = _graphClient.ServicePrincipals[$"{spObject.Id}"].Owners
                                                                      .Request()
                                                                      .GetAsync();
                taskOwners.Wait();
                result = taskOwners.Result.CurrentPage.Where(x => (x as User).UserPrincipalName != null).ToDictionary(x => (x as User).DisplayName, x => (x as User).UserPrincipalName);
            }

            return result;
        }

        internal static void CreateAADUsersAsync(string userNamePattern, int count, int lowerLimit = 1)
        {
            var usersTasks = new List<Task>();

            string domainName = GetDomainName();

            for (int i = 1; i <= count; i++)
            {
                string userID = $"{userNamePattern}-{lowerLimit}";
                var user = new User
                {
                    AccountEnabled = false,
                    DisplayName = userID,
                    MailNickname = userID,
                    UserPrincipalName = $"{userID}@{domainName}",
                    PasswordProfile = new PasswordProfile
                    {
                        ForceChangePasswordNextSignIn = true,
                        Password = "password-value".GenerateToken()
                    }
                };


                Task<User> userTask = _graphClient.Users.Request().AddAsync(user);
                usersTasks.Add(userTask);
                lowerLimit++;

            };

            Task.WaitAll(usersTasks.ToArray());

            Console.WriteLine("User Object creation done, press a key to continue");
        }

        public static string GetDomainName()
        {
            Task<IGraphServiceDomainsCollectionPage> domains = _graphClient.Domains.Request().GetAsync();

            domains.Wait();

            return domains.Result.FirstOrDefault().Id;

        }

        internal static async Task<List<User>> GetAllUsers(string userNamePattern, int count = 0)
        {
            try
            {
                List<User> usersList = new List<User>();


                var usersPage = await _graphClient.Users
               .Request()
               .Filter($"startswith(displayName,'{userNamePattern}')")
               .GetAsync();


                usersList.AddRange(usersPage.CurrentPage);

                bool breakOnListCountGreaterOrEqualsToCount = (count > 0);

                while (usersPage.NextPageRequest != null)
                {
                    if (breakOnListCountGreaterOrEqualsToCount && usersList.Count >= count)
                    {
                        break;
                    }
                    usersPage = await usersPage.NextPageRequest.GetAsync();
                    usersList.AddRange(usersPage.CurrentPage);
                }

                if (count > 0)
                    return usersList.Take(count).ToList();
                else
                    return usersList.ToList();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting All Users: {ex.Message}");
                return null;
            }
        }

        internal static bool AreValidAADUsers(string spNotes)
        {

            List<string> spNotesAsList = spNotes.Split(';').ToList();

            try
            {
                List<User> usersList = new List<User>();

                Parallel.ForEach(spNotesAsList, userEmail =>
                {
                    var usersPage =  _graphClient.Users
                    .Request()
                    //.Filter($"startswith(userPrincipalName,'{userEmail}')")
                    .Filter($"userPrincipalName eq '{userEmail}'")
                    .GetAsync();

                    usersList.AddRange(usersPage.Result);
                });

                return usersList.Count == usersList.Count;

            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error Checking if AreValidAADUsers: {ex.Message}");
                throw;
            }
        }


        internal static void ClearNotesField(List<ServicePrincipal> targetServicePrincipals)
        {
            if (targetServicePrincipals != null && targetServicePrincipals.Count() > 0)
            {
                var tasks = new List<Task<ServicePrincipal>>();

                //foreach(var spObject in targetServicePrincipals)
                Parallel.ForEach(targetServicePrincipals, spObject =>
                {
                    var servicePrincipal = new ServicePrincipal
                    {
                        AdditionalData = new Dictionary<string, object>()
                    };

                    // Null out notes for target Service Principal object to keep it in sync and save a couple API calls
                    spObject.Notes = null;

                    servicePrincipal.AdditionalData.Add("notes", null);

                    Task<ServicePrincipal> thisTask = _graphClient.ServicePrincipals[spObject.Id].Request().UpdateAsync(servicePrincipal);

                    tasks.Add(thisTask);

                });

                Task.WaitAll(tasks.ToArray());

            }
        }

        internal static void UpdateNotesFieldWithValidEmail(List<ServicePrincipal> targetServicePrincipals)
        {
            if (targetServicePrincipals != null && targetServicePrincipals.Count() > 0)
            {
                var tasks = new List<Task<ServicePrincipal>>();

                var validFormattedEmails = GetValidFormattedEmails();

                Parallel.ForEach(targetServicePrincipals, spObject =>
                {
                    var servicePrincipal = new ServicePrincipal
                    {
                        Notes = validFormattedEmails
                    };

                    // Update Notes for target Service Principal object to keep it in sync and save a couple API calls
                    spObject.Notes = validFormattedEmails;

                    Task<ServicePrincipal> thisTask = _graphClient.ServicePrincipals[spObject.Id].Request().UpdateAsync(servicePrincipal);
                    tasks.Add(thisTask);

                });

                Task.WaitAll(tasks.ToArray());

            }
        }

        private static string GetValidFormattedEmails()
        {
            List<string> emaiList = new List<string>();

            Random random = new Random();
 
            for (int i = 1; i <= _emailSettings.TestEmailCount; i++)
            {
                if (_emailSettings.IncludeRandomStringToTestEmail)
                {
                    emaiList.Add($"{_emailSettings.TestEmailBase.AddRandomStringToEmail()}-{i}@{_emailSettings.TestEmailDomainNames[random.Next(0, _emailSettings.TestEmailDomainNames.Count)]}");
                }
                else
                {
                    emaiList.Add($"{_emailSettings.TestEmailBase}-{i}@{_emailSettings.TestEmailDomainNames[random.Next(0, _emailSettings.TestEmailDomainNames.Count)]}");
                }

            }

            return string.Join(";", emaiList.Select(x => x));
        }

        internal static void UpdateNotesFieldWithAADOwnersEmail(List<ServicePrincipal> targetServicePrincipals)
        {
            if (targetServicePrincipals != null && targetServicePrincipals.Count() > 0)
            {
                var tasks = new List<Task<ServicePrincipal>>();

                Parallel.ForEach(targetServicePrincipals, spObject => 
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

                    // Update Notes for target Service Principal object to keep it in sync and save a couple API calls
                    spObject.Notes = semicolonSeparatedOwnersEmail;

                    Task<ServicePrincipal> thisTask = _graphClient.ServicePrincipals[spObject.Id].Request().UpdateAsync(servicePrincipal);

                    tasks.Add(thisTask);
                });

                Task.WaitAll(tasks.ToArray());
            }
        }
      
        internal static async Task<List<ServicePrincipal>> GetAllServicePrincipals(string spNamePefix, int count = 0)
        {
            try
            {
                List<ServicePrincipal> servicePrincipalList = new List<ServicePrincipal>();


                var servicePrincipalsPage = await _graphClient.ServicePrincipals
               .Request()
               .Filter($"startswith(displayName,'{spNamePefix}')")
               .GetAsync();

 
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

            var serviceTasks = new List<Task<ServicePrincipal>>();


            for (int i = 1; i <= count; i++)
            {

                var application = new Application
                {
                    DisplayName = $"{spNamePefix}-{lowerLimit}"
                };

                Task<Application> appTask = _graphClient.Applications.Request().AddAsync(application);
                appTask.Wait();

                var servicePrincipal = new ServicePrincipal
                {
                    AppId = appTask.Result.AppId
                };


                Task<ServicePrincipal> spTask = _graphClient.ServicePrincipals.Request().AddAsync(servicePrincipal);
                serviceTasks.Add(spTask);

                lowerLimit++;

            };

            Task.WaitAll(serviceTasks.ToArray());

            Console.WriteLine("app registration and service principal creation done, press a key to continue");

        }

   
        private static void AddServicePrincipals(string spNamePefix)
        {

            var applicationsList = GetAllApplicationAsync(spNamePefix).Result;
            var servicePrinciaplTasks = new List<Task<ServicePrincipal>>();

            foreach(var app in applicationsList)
            {
                var servicePrincipal = new ServicePrincipal
                {
                    AppId = app.AppId
                };

                Task<ServicePrincipal> thisTask = _graphClient.ServicePrincipals.Request().AddAsync(servicePrincipal);

                servicePrinciaplTasks.Add(thisTask);
            }

            Task.WaitAll(servicePrinciaplTasks.ToArray());
        }

        internal static async Task GetUsersAsync()
        {
            var selectFields = new[] { "appId", "displayName", "notes", "owners", "notificationEmailAddresses" };
            var users = await _graphClient.Users
                .Request()

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

                    Task thisTask = _graphClient.ServicePrincipals[spObject.Id].Request().DeleteAsync();
                    tasks.Add(thisTask);
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

                    Task thisTask = _graphClient.Applications[appObject.Id].Request().DeleteAsync();
                    tasks.Add(thisTask);
                }

                Task.WaitAll(tasks.ToArray());
            }
            Console.WriteLine("DeleteRegisteredApplicationsAsync done");

        }

        internal static void UpdateServicePrincipalNote(string servicePrincipalNote, IList<ServicePrincipal> servicePrincipalList)
        {

            if (servicePrincipalList != null && servicePrincipalList.Count() > 0)
            {
                var tasks = new List<Task<ServicePrincipal>>();

                Parallel.ForEach(servicePrincipalList, spObject =>
                {

                    var servicePrincipal = new ServicePrincipal
                    {
                        Notes = $"{servicePrincipalNote} - {spObject.Id}"
                    };

                    Task<ServicePrincipal> thisTask = _graphClient.ServicePrincipals[spObject.Id].Request().UpdateAsync(servicePrincipal);
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
