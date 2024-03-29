﻿using System;
using System.Threading.Tasks;
using CSE.Automation.DataAccess;
using CSE.Automation.Model;
using Microsoft.Graph;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ObjectTrackingState
{
    internal class ObjectStateDefinition2_2 : ObjectStateDefinitionBase, IObjectStateDefinition
    {
        public ObjectStateDefinition2_2(ServicePrincipal servicePrincipal, ServicePrincipalModel servicePrincipalModel,
                                    ObjectTrackingRepository objectTrackingRepository, ActivityContext activityContext, IInputGenerator inputGenerator) 
                                    : base(servicePrincipal, servicePrincipalModel, objectTrackingRepository, activityContext, inputGenerator)
        {
        }

        public override bool Validate()
        {
            //ObjectTracking Item must  exist 
            if (ObjectTrackingItemExists())
            {
                return true;
            }
            else
            {
                //Create ObjectTracking item 

                var now = DateTimeOffset.Now;

                var objectModel = new TrackingModel<ServicePrincipalModel>
                {
                    CorrelationId = Context.CorrelationId,
                    Created = now,
                    LastUpdated = now,
                    TypedEntity = SPModel,
                };


                Repository.GenerateId(objectModel);

                Task<TrackingModel> createTask = Task.Run(() => Repository.UpsertDocumentAsync(objectModel));
                createTask.Wait();

                return createTask.Result.Id == ServicePrincipalObject.Id;
                
            }
        }
    }
}
