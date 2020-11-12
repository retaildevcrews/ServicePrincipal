// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CSE.Automation.Interfaces
{
    internal interface ICosmosDBSettings : ISettingsValidator
    {
        string Uri { get; }
        string Key { get; }
        string DatabaseName { get; }
    }
}
