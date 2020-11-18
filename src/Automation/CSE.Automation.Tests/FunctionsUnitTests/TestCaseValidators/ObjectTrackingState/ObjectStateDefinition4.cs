using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CSE.Automation.DataAccess;
using CSE.Automation.Model;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ObjectTrackingState
{
    internal class ObjectStateDefinition4 : ObjectStateDefinitionBase, IObjectStateDefinition
    {
        public ObjectStateDefinition4(ServicePrincipal servicePrincipal, ServicePrincipalModel servicePrincipalModel,
                                    ObjectTrackingRepository objectTrackingRepository, ActivityContext activityContext, InputGenerator inputGenerator) 
                                    : base(servicePrincipal, servicePrincipalModel, objectTrackingRepository, activityContext, inputGenerator)
        {
        }

        public override bool Validate()
        {
            var assignedOwnersList = GetAssignedOwnersTestCase4Only();
            //ObjectTracking Item must  exist 
            if (ObjectTrackingItemExistsAndHasNotesSetToOwnersTestCase4Only(assignedOwnersList))
            {
                return true;
            }
            else
            {
                //Create ObjectTracking item 

                var now = DateTimeOffset.Now;

                SPModel.Owners = assignedOwnersList;

                SPModel.Notes = string.Join(';', assignedOwnersList);

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
