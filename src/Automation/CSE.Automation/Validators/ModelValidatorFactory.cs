using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;

namespace CSE.Automation.Model
{

    interface IModelValidator<T>
    {
        ValidationResult Validate(T model);
    }

    interface IModelValidatorFactory {
        IEnumerable<IModelValidator<TEntity>> Get<TEntity>();
    }

    class ModelValidatorFactory : IModelValidatorFactory
    {
        IServiceProvider _serviceProvider;
        public ModelValidatorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        public IEnumerable<IModelValidator<T>> Get<T>()
        {
            return _serviceProvider.GetServices<IModelValidator<T>>();
        }
    }
}

