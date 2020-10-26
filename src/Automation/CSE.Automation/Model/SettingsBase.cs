using System;
using System.Linq;
using System.Runtime.CompilerServices;
using CSE.Automation.Interfaces;

namespace CSE.Automation.Model
{

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    internal sealed class SecretAttribute : Attribute
    {
        private readonly string _name;

        public SecretAttribute(string name)
        {
            _name = name.Trim('%');
        }

        public string SecretName => _name;
    }


    public abstract class SettingsBase : ISettingsValidator
    {
        private readonly ISecretClient _secretClient;

        protected SettingsBase(ISecretClient secretClient)
        {
            _secretClient = secretClient;
        }

        protected string GetSecret([CallerMemberName] string name = default)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var propInfo = GetType().GetProperty(name);
            if (propInfo == null)
            {
                throw new ArgumentOutOfRangeException($"Failed to get property information for property '{name}'");
            }

            var attr = propInfo.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(SecretAttribute));

            return attr != null
                ? GetSecretInternal(attr.ConstructorArguments.First().Value.ToString())
                : GetSecretInternal(propInfo.Name);

        }

        private string GetSecretInternal(string name)
        {
            return _secretClient.GetSecretValue(name);
        }

        public virtual void Validate() { }
    }

}
