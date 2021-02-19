using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSE.Automation.Model;
using CSE.Automation.Tests.IntegrationTests.TestCaseValidators.DataAccess;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.AuditResults
{
    internal class AuditValidationManager : IResultsManager, IDisposable
    {
        private readonly IInputGenerator _inputGenerator;
        private readonly AuditRepositoryTest _auditRepositoryTest;
        private ActivityContext _activityContext;

        private AuditEntry _savedAuditEntry;

        public AuditValidationManager(IInputGenerator inputGenerator, AuditRepositoryTest auditRepositoryTest, ActivityContext activityContext)
        {
            _inputGenerator = inputGenerator;
            _auditRepositoryTest = auditRepositoryTest;
            _activityContext = activityContext;

            SaveState();
        }

        public void SaveState()
        {
            _savedAuditEntry = GetMostRecentAuditEntryItem();
        }

        private AuditEntry GetMostRecentAuditEntryItem()
        {
            Task<IEnumerable<AuditEntry>> getAuditItems = Task.Run(() => _auditRepositoryTest.GetMostRecentAsync(_inputGenerator.GetServicePrincipal().Id));
            getAuditItems.Wait();

            AuditEntry result = null;
            var dataResult = getAuditItems.Result.ToList();
            if (dataResult.Count > 0)
            {
                result = dataResult[0];
            }

            return result;
        }

        public bool Validate()
        {
            string resultValidatorClassName = _inputGenerator.TestCaseCollection.GetAuditValidator(_inputGenerator.TestCaseId); 

            var servicePrincipal = _inputGenerator.GetServicePrincipal();

            string objectToInstantiate = $"CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.AuditResults.{resultValidatorClassName}, CSE.Automation.Tests";

            var objectType = Type.GetType(objectToInstantiate);

            var newAuditEntry = GetMostRecentAuditEntryItem();

            object[] args = { _savedAuditEntry, newAuditEntry, _activityContext, servicePrincipal, _auditRepositoryTest,  _inputGenerator.TestCaseId};

            var instantiatedObject = Activator.CreateInstance(objectType, args) as IAuditResultValidator;

            return instantiatedObject.Validate();

        }
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
