using System;
using FluentValidation;

namespace CSE.Automation.Validators
{
  public class ModelValidatorRules
    {
        protected bool BeValidJson(string json)
        {
            return true;
        }

        protected bool BeValidEmail(string email)
        {
            return true;
        }
        protected bool BeValidAadUser(string user)
        {
            return true;
        }
    }
}
