using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSE.Automation.DataAccess;
using CSE.Automation.Model;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ConfigurationResults
{
    internal class ConfigurationValidationManager : IResultsManager, IDisposable
    {
        private readonly IInputGenerator _inputGenerator;
        private readonly ConfigRepository _configRepository;
        private ActivityContext _activityContext;

        private ProcessorConfiguration _savedConfigEntry;

        public ConfigurationValidationManager(IInputGenerator inputGenerator, ConfigRepository configRepository, ActivityContext activityContext)
        {
            _inputGenerator = inputGenerator;
            _configRepository = configRepository;
            _activityContext = activityContext;

            SaveState();
        }

        public void SaveState()
        {
            _savedConfigEntry = GetConfigItem(true);
        }

        private ProcessorConfiguration GetConfigItem(bool unlock = false)
        {

            ProcessorConfiguration configuration = _configRepository.GetByIdAsync(_inputGenerator.ConfigId, ProcessorType.ServicePrincipal.ToString()).GetAwaiter().GetResult();

            if (unlock && configuration.IsProcessorLocked)
            {
                configuration.IsProcessorLocked = false;

                configuration = _configRepository.UpsertDocumentAsync(configuration).GetAwaiter().GetResult();
            }

            return configuration;

        }

        public bool Validate()
        {
            string resultValidatorClassName = _inputGenerator.TestCaseCollection.GetConfigValidator(_inputGenerator.TestCaseId); 

            
            string objectToInstantiate = $"CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ConfigurationResults.{resultValidatorClassName}, CSE.Automation.Tests";

            var objectType = Type.GetType(objectToInstantiate);

            var newConfigEntry = GetConfigItem();

            object[] args = { _savedConfigEntry, newConfigEntry, _activityContext, _configRepository,  _inputGenerator.TestCaseId};

            var instantiatedObject = Activator.CreateInstance(objectType, args) as IConfigResultValidator;

            return instantiatedObject.Validate();

        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
