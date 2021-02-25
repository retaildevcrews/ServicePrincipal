// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.Linq;
using CSE.Automation.Extensions;
using CSE.Automation.Interfaces;
using FluentValidation;
using FluentValidation.Results;
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
                        var (sp, _) = graphHelper.GetEntityWithOwners(token).Result;
                        if (sp is null)
                        {
                            var failure = new ValidationFailure("Notes", string.Format(CultureInfo.CurrentCulture, AuditCode.InvalidDirectoryUPN.Description(), token), field) { ErrorCode = AuditCode.InvalidDirectoryUPN.ToString() };
                            context.AddFailure(failure);
                        }
                    });
                });

            // RuleFor(m => m.Owners)
            //     .NotEmpty();
        }
    }
}
