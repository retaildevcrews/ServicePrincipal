// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CSE.Automation.Interfaces;
using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;

namespace CSE.Automation.Model
{
    internal class ModelValidatorFactory : IModelValidatorFactory
    {
        private IServiceProvider serviceProvider;
        public ModelValidatorFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IEnumerable<IModelValidator<T>> Get<T>()
        {
            return this.serviceProvider.GetServices<IModelValidator<T>>();
        }
    }
}
