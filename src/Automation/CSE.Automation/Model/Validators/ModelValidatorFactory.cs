// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using CSE.Automation.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CSE.Automation.Model.Validators
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
