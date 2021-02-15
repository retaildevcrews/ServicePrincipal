using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSE.Automation.DataAccess;
using CSE.Automation.Model;
using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;
using Microsoft.Graph;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ObjectTrackingState
{
    abstract class ObjectStateDefinitionBase : IObjectStateDefinition
    {

        public ObjectTrackingRepository Repository { get; }
        public ActivityContext Context { get; }
        public ServicePrincipal ServicePrincipalObject { get; }

        public ServicePrincipalModel SPModel { get; }

        private IInputGenerator _inputGenerator;


        public ObjectStateDefinitionBase(ServicePrincipal servicePrincipal, ServicePrincipalModel servicePrincipalModel, 
                            ObjectTrackingRepository objectTrackingRepository, ActivityContext activityContext, IInputGenerator inputGenerator)
        {
            ServicePrincipalObject = servicePrincipal;
            SPModel = servicePrincipalModel;

            Repository = objectTrackingRepository;

            Context = activityContext;

            _inputGenerator = inputGenerator;

        }

        public abstract bool Validate();


        protected bool ObjectTrackingItemExists()
        {
            Task<TrackingModel> getObjectTrackingItem = Task.Run(() => Repository.GetByIdAsync(ServicePrincipalObject.Id, "ServicePrincipal"));
            getObjectTrackingItem.Wait();

            return getObjectTrackingItem.Result != null;

        }

        protected bool ObjectTrackingItemExistsAndHasNotesSetToOwnersTestCase4Only(List<string> assignedOwnersList)
        {
            bool result = false;
            Task<TrackingModel> getObjectTrackingItem = Task.Run(() => Repository.GetByIdAsync(ServicePrincipalObject.Id, "ServicePrincipal"));
            getObjectTrackingItem.Wait();

            if (getObjectTrackingItem.Result != null && getObjectTrackingItem.Result.Entity != null)
            {
                ServicePrincipalModel spModel = TrackingModel.Unwrap<ServicePrincipalModel>(getObjectTrackingItem.Result);

                if (!string.IsNullOrEmpty(spModel.Notes))
                {
                    Dictionary<string,string> ownersInfoList = GraphHelper.GetOwnersDisplayNameAndUserPrincipalNameKeyValuePair(ServicePrincipalObject);
                    
                    List<string> ownersList = ownersInfoList.Values.ToList();

                    if (ownersList.Count == 0)// this Function was written for TC4 
                    {
                        ownersList.AddRange(assignedOwnersList);
                    }

                    var currentNotes = spModel.Notes.Split(";").ToList();

                    result = ownersList.Count() == currentNotes.Count() && ownersList.Except(currentNotes).Count() == 0;
                }
            }
            return result;
        }

        protected string GetServicePrincipalOwnersAsString()
        {
            Dictionary<string,string> ownersInfoList = GraphHelper.GetOwnersDisplayNameAndUserPrincipalNameKeyValuePair(ServicePrincipalObject);

            List<string> ownersList = ownersInfoList.Values.ToList();

            return string.Join(';', ownersList);
        }

        protected List<string> GetAssignedOwnersTestCase4Only()
        {
            string usersPrefix = _inputGenerator.AadUserServicePrincipalPrefix;

            var userslList = GraphHelper.GetAllUsers($"{usersPrefix}").Result;

            var toBeAssigned = ((EvaluateInputGenerator)_inputGenerator).TC4AssignTheseOwnersWhenCreatingAMissingObjectTracking.GetAsList();

            List<string> spUsers = new List<string>();
            foreach(var userName in toBeAssigned)
            {
                string userPrincipalName = userslList.FirstOrDefault(x => x.DisplayName == userName.Trim())?.UserPrincipalName;

                if (string.IsNullOrEmpty(userPrincipalName))
                {
                    throw new InvalidDataException($"Unable to get AAD User for assigned Owner user [{userName}].");
                }
                else
                {
                    spUsers.Add(userPrincipalName);
                }
            }

            return spUsers;
        }
    }
}
