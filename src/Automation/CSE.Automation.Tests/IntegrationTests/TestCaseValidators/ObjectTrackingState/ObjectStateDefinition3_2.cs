﻿using System.Threading.Tasks;
using CSE.Automation.DataAccess;
using CSE.Automation.Model;
using Microsoft.Graph;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ObjectTrackingState
{
    internal class ObjectStateDefinition3_2 : ObjectStateDefinitionBase, IObjectStateDefinition
    {
        public ObjectStateDefinition3_2(ServicePrincipal servicePrincipal, ServicePrincipalModel servicePrincipalModel,
                                    ObjectTrackingRepository objectTrackingRepository, ActivityContext activityContext, IInputGenerator inputGenerator) 
                                    : base(servicePrincipal, servicePrincipalModel, objectTrackingRepository, activityContext, inputGenerator)
        {
        }

        public override bool Validate()
        {
            //ObjectTracking Item must  exist 
            if (ObjectTrackingItemExists())
            {
                Task<TrackingModel> deleteTask = Task.Run(() => Repository.DeleteDocumentAsync(ServicePrincipalObject.Id, ProcessorType.ServicePrincipal.ToString()));
                deleteTask.Wait();

                return deleteTask.Result == null;
            }
            else
            {
                return true;
                
            }
        }
    }
}
