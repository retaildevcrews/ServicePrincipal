using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSE.Automation.DataAccess;
using CSE.Automation.Model;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.AuditResults
{
    internal class AuditValidationManager : IResultsManager, IDisposable
    {
        private readonly InputGenerator _inputGenerator;
        private readonly AuditRepository _auditRepository;
        private ActivityContext _activityContext;

        private AuditEntry _savedAuditEntry;

        public AuditValidationManager(InputGenerator inputGenerator, AuditRepository auditRepository, ActivityContext activityContext)
        {
            _inputGenerator = inputGenerator;
            _auditRepository = auditRepository;
            _activityContext = activityContext;

            SaveState();
        }

        public void SaveState()
        {
            _savedAuditEntry = GetMostRecentAuditEntryItem();
        }

        private AuditEntry GetMostRecentAuditEntryItem()
        {
            Task<IEnumerable<AuditEntry>> getAuditItems = Task.Run(() => _auditRepository.GetMostRecentAsync(_inputGenerator.GetServicePrincipal().Id));
            getAuditItems.Wait();

            AuditEntry result = null;
            var dataResult = getAuditItems.Result.ToList();
            if (dataResult.Count > 0)
            {
                result = dataResult[0];
            }
            else
            {
                throw new Exception($"Unable to Get the most recent Audit item  for Test Case Id: {_inputGenerator.TestCaseId}");
            }

            return result;
        }

        public bool Validate()
        {
            string resultValidatorClassName = _inputGenerator.TestCaseId.GetAuditValidator();


            string objectToInstantiate = $"CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.AuditResults.{resultValidatorClassName}, CSE.Automation.Tests";

            var objectType = Type.GetType(objectToInstantiate);

            var newAuditEntry = GetMostRecentAuditEntryItem();

            object[] args = { _savedAuditEntry, newAuditEntry, _activityContext, _inputGenerator.TestCaseId};

            var instantiatedObject = Activator.CreateInstance(objectType, args) as IAuditResultValidator;

            return instantiatedObject.Validate();
          
        }
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
