using CSE.Automation.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation.Processors
{
    // TODO: Examine this and IDALResolve and see if they may be refactored to a single class.
    public class ProcessorResolver:IServiceResolver
    {
        private IDAL _configDAL;
        private ConcurrentDictionary<string, IDeltaProcessor> _registeredProcessors = new System.Collections.Concurrent.ConcurrentDictionary<string, IDeltaProcessor>();

        public ProcessorResolver(IDAL configDAL)
        {
            _configDAL = configDAL;
        }

        private IDeltaProcessor CreateProcessor (ProcessorType processorType)
        {

            IDeltaProcessor returnProcessor = default;

            switch (processorType)
            {
                case ProcessorType.ServicePrincipal:
                    returnProcessor = new ServicePrincipalProcessor(_configDAL);
                    break;
                case ProcessorType.User:
                    throw new NotImplementedException();

            }

            return returnProcessor;
        }

        public T GetService<T>(string keyName)
        {
            if (typeof(IDeltaProcessor) != typeof(T))
                throw new InvalidCastException("For ProcessorResolver type T must be of type IDeltaProcessor");

            ProcessorType processorType = Enum.Parse<ProcessorType>(keyName);

            return (T)_registeredProcessors.GetOrAdd(keyName, CreateProcessor(processorType));
        }
    }
}
