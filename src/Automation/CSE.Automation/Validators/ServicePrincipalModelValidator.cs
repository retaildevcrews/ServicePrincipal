// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CSE.Automation.Graph;
using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CSE.Automation.Validators
{
    internal class ServicePrincipalModelValidator : AbstractValidator<ServicePrincipalModel>, IModelValidator<ServicePrincipalModel>
    {
        public ServicePrincipalModelValidator(IGraphHelper<User> graphHelper)
        {
            Include(new GraphModelValidator());

            RuleFor(m => m.Notes)
                .NotEmpty()
                .HasOnlyEmailAddresses()
                .Custom((field, context) =>
                {
                    field?.Split(',', ';').ToList().ForEach(token =>
                    {
                        if (graphHelper.GetGraphObjectWithOwners(token).Result is null)
                        {
                            context.AddFailure($"'{token}' is not a valid UserPrincipalName in this directory");
                        }
                    });
                });
            RuleFor(m => m.Owners)
                .NotEmpty();
        }
    }
}
