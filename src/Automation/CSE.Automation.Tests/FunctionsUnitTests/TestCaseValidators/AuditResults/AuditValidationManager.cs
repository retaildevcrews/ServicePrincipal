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

        private AuditEntry _savedAuditEntry;

        public AuditValidationManager(InputGenerator inputGenerator, AuditRepository auditRepository)
        {
            _inputGenerator = inputGenerator;
            _auditRepository = auditRepository;

            SaveState();
        }

        public void SaveState()
        {
            Task<IEnumerable<AuditEntry>> getAuditItems = Task.Run(() => _auditRepository.GetMostRecentAsync(_inputGenerator.GetServicePrincipal().Id));
            getAuditItems.Wait();

            var dataResult = getAuditItems.Result.ToList();
            if (dataResult.Count > 0)
            {
                _savedAuditEntry = dataResult[0];
            }

        }
        public bool Validate()
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
