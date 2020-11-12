// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CSE.Automation.Interfaces;
using CSE.Automation.Model;
using System.Configuration;
using SettingsBase = CSE.Automation.Model.SettingsBase;

namespace CSE.Automation.DataAccess
{
    internal class CosmosDBSettings : SettingsBase, ICosmosDBSettings
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
