// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CSE.Automation.Graph
{
    internal interface IGraphHelperSettings
    {
        string GraphAppClientId { get; }
        string GraphAppTenantId { get; }
        string GraphAppClientSecret { get; }
        void Validate();
    }
}
