using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using System.Configuration;
using SettingsBase = CSE.Automation.Model.SettingsBase;

namespace CSE.Automation.DataAccess
{
    class CosmosDBSettings : SettingsBase, ICosmosDBSettings
    {
        private string uri;
        private string key;
        private string databaseName;

        public CosmosDBSettings(ISecretClient secretClient)
            : base(secretClient)
        {
        }

        [Secret(Constants.CosmosDBURLName)]
        public string Uri
        {
            get { return uri ?? GetSecret(); }
            set { uri = value; }
        }

        [Secret(Constants.CosmosDBKeyName)]
        public string Key
        {
            get { return key ?? GetSecret(); }
            set { key = value; }
        }

        [Secret(Constants.CosmosDBDatabaseName)]
        public string DatabaseName
        {
            get { return databaseName ?? GetSecret(); }
            set { databaseName = value; }
        }

        public override void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.Uri))
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: Uri is invalid");
            }

            if (string.IsNullOrWhiteSpace(this.Key))
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: Key is invalid");
            }

            if (string.IsNullOrWhiteSpace(this.DatabaseName))
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: DatabaseName is invalid");
            }
        }
    }
}
