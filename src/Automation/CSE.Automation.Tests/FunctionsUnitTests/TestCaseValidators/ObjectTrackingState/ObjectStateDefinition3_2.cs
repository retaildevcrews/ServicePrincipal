using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSE.Automation.DataAccess;
using CSE.Automation.Model;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ObjectTrackingState
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
                Task<TrackingModel> deleteTask = Task.Run(() => Repository.DeleteDocumentAsync(ServicePrincipalObject.Id, "ServicePrincipal"));
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
