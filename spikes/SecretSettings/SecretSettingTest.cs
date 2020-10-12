
class App
{
    public static Main(string[] args)
    {
        var settings = new GraphHelperSettings()
        {
            GraphAppTenantId = Guid.Emtpy.ToString();
            GraphAppClientId = "123";
        };

        Debug.WriteLine(settings.GraphAppClientSecret);
        Debug.WriteLine(settings.GraphAppClientId);
    }
}

    class GraphHelperSettings : SettingsBase
    {
        private string _graphAppClientId;
        private string _graphAppTenantId;
        private string _graphAppClientSecret;

        public GraphHelperSettings() : base(null) { }

        public string GraphAppClientId
        {
            get => base.GetProperty();
            set => _graphAppClientId = value;
        }

        public string GraphAppTenantId
        {
            get => base.GetProperty();
            set => _graphAppTenantId = value;
        }

        [Secret("graphAppClientSecret")]
        public string GraphAppClientSecret
        {
            get => base.GetProperty();
            set => _graphAppClientSecret = value;
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    sealed class SecretAttribute : Attribute
    {
        private string _name;

        public SecretAttribute(string name)
        {
            _name = name;
        }

        public string SecretName { get => _name;  }
    }


    abstract class SettingsBase
    {
        private readonly object _secretClient;

        protected SettingsBase(object secretClient)
        {
            _secretClient = secretClient;
        }

        protected string GetProperty([CallerMemberName]string name=default)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            var propInfo = this.GetType().GetProperty(name);
            if (propInfo == null)
            {
                throw new ArgumentOutOfRangeException($"Failed to get property information for property '{name}'");
            }

            var propValue = propInfo.GetValue(this)?.ToString();
            var attr = propInfo.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(SecretAttribute));

            return attr != null
                ? GetSecret(attr.NamedArguments.First().TypedValue.Value.ToString()) 
                : propValue;

        }

        protected string GetSecret(string name)
        {
            return $"Calling secret service for {name}";
            //return _secretClient.GetSecretValue(name);
        }
    }
