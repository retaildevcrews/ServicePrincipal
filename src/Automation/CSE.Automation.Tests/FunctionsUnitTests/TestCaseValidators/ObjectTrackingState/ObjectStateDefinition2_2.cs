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
    internal class ObjectStateDefinition2_2 : ObjectStateDefinitionBase, IObjectStateDefinition
    {
        public ObjectStateDefinition2_2(ServicePrincipal servicePrincipal, ServicePrincipalModel servicePrincipalModel,
                                    ObjectTrackingRepository objectTrackingRepository, ActivityContext activityContext, InputGenerator inputGenerator) 
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

                Task<TrackingModel> deleteTask = Task.Run(() => Repository.UpsertDocumentAsync(objectModel));
                deleteTask.Wait();

                return deleteTask.Result.Id == ServicePrincipalObject.Id;
                
            }
        }
    }
}
