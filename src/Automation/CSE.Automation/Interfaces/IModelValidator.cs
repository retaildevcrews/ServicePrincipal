// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;

namespace CSE.Automation.Interfaces
{
    internal interface IModelValidator<T>
    {
        ValidationResult Validate(T model);
    }
}
