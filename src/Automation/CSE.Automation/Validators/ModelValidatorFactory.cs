using System;
using System.Collections.Generic;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;

namespace CSE.Automation.Model
{
    internal interface IModelValidator<T>
    {
        ValidationResult Validate(T model);
    }

    internal interface IModelValidatorFactory
    {
        IEnumerable<IModelValidator<TEntity>> Get<TEntity>();
    }

    internal class ModelValidatorFactory : IModelValidatorFactory
    {
        private readonly IServiceProvider _serviceProvider;
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
