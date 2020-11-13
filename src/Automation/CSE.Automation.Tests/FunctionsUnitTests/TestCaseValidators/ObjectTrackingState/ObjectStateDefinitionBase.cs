using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSE.Automation.DataAccess;
using CSE.Automation.Model;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ObjectTrackingState
{
    abstract class ObjectStateDefinitionBase : IObjectStateDefinition
    {

        public ObjectTrackingRepository Repository { get; }
        public ActivityContext Context { get; }
        public ServicePrincipal ServicePrincipalObject { get; }

        public ServicePrincipalModel SPModel { get; }
        

        public ObjectStateDefinitionBase(ServicePrincipal servicePrincipal, ServicePrincipalModel servicePrincipalModel, 
                                        ObjectTrackingRepository objectTrackingRepository, ActivityContext activityContext)
        {
            ServicePrincipalObject = servicePrincipal;
            SPModel = servicePrincipalModel;

            Repository = objectTrackingRepository;

            Context = activityContext;

        }

        public abstract bool Validate();


        protected bool ObjectTrackingItemExists()
        {
            Task<TrackingModel> getObjectTrackingItem = Task.Run(() => Repository.GetByIdAsync(ServicePrincipalObject.Id, "ServicePrincipal"));
            getObjectTrackingItem.Wait();

            return getObjectTrackingItem.Result != null;

        }
    }
}
