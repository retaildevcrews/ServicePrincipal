using System.Threading.Tasks;
using CSE.Automation.DataAccess;
using CSE.Automation.Model;
using Microsoft.Graph;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.ObjectTrackingState
{
    internal class ObjectStateDefinition2 : ObjectStateDefinitionBase, IObjectStateDefinition
    {
        public ObjectStateDefinition2(ServicePrincipal servicePrincipal, ServicePrincipalModel servicePrincipalModel,
                                      ObjectTrackingRepository objectTrackingRepository, ActivityContext activityContext, IInputGenerator inputGenerator)
                                    : base(servicePrincipal, servicePrincipalModel, objectTrackingRepository, activityContext, inputGenerator)
        {
        }


        public override bool Validate()
        {
            //ObjectTracking Item must NOT exist 

            if (ObjectTrackingItemExists())
            {

                Task<TrackingModel> deleteTask = Task.Run(() => Repository.DeleteDocumentAsync(ServicePrincipalObject.Id, ProcessorType.ServicePrincipal.ToString()));
                deleteTask.Wait();

                return deleteTask.Result == null;
            }
            else
            {
                return true;// Object does not exist
            }
        }
    }
}
