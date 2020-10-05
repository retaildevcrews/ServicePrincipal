using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Spike
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = new GraphHelperSettings()
            {
                GraphAppTenantId = Guid.Empty.ToString(),
                GraphAppClientId = "123"
            };

            Debug.WriteLine(settings.GraphAppClientSecret);
            Debug.WriteLine(settings.GraphAppClientId);
        }
    }

     class GraphHelperSettings : SettingsBase
    {
        public GraphHelperSettings() : base(null) { }

        public string GraphAppClientId { get; set; }

        public string GraphAppTenantId { get; set; }

        [Secret("graphAppClientSecret")]
        public string GraphAppClientSecret => base.GetSecret();
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    sealed class SecretAttribute : Attribute
    {
        private readonly string _name;

        public SecretAttribute(string name)
        {
            _name = name;
        }

        public string SecretName => _name;
    }


    abstract class SettingsBase
    {
        private readonly object _secretClient;

        protected SettingsBase(object secretClient)
        {
            _secretClient = secretClient;
        }

        protected string GetSecret([CallerMemberName]string name=default)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            var propInfo = this.GetType().GetProperty(name);
            if (propInfo == null)
            { 
                throw new ArgumentOutOfRangeException($"Failed to get property information for property '{name}'");
            }

            var attr = propInfo.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(SecretAttribute));

            return attr != null
                ? GetSecretInternal(attr.ConstructorArguments.First().Value.ToString()) 
                : GetSecretInternal(propInfo.Name);

        }

        protected string GetSecretInternal(string name)
        {
            return $"Calling secret service for {name}";
            //return _secretClient.GetSecretValue(name);
        }
    }

}
