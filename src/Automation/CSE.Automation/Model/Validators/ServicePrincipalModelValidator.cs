// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using CSE.Automation.Interfaces;
using FluentValidation;
using Microsoft.Graph;

namespace CSE.Automation.Model.Validators
{
    internal class ServicePrincipalModelValidator : AbstractValidator<ServicePrincipalModel>, IModelValidator<ServicePrincipalModel>
    {
        public static char[] NotesSeparators = { ',', ';' };

        public ServicePrincipalModelValidator(IGraphHelper<User> graphHelper)
        {
            Include(new GraphModelValidator());

            RuleFor(m => m.Notes)
                .NotEmpty()
                .HasOnlyEmailAddresses()
                .Custom((field, context) =>
                {
                    field?.Split(NotesSeparators).Select(x => x.Trim()).ToList().ForEach(token =>
                    {
                        if (graphHelper.GetGraphObjectWithOwners(token).Result is null)
                        {
                            context.AddFailure($"'{token}' is not a valid UserPrincipalName in this directory");
                        }
                    });
                });

            // RuleFor(m => m.Owners)
            //     .NotEmpty();
        }
    }
}
