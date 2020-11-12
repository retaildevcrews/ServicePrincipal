// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace CSE.Automation.Interfaces
{
    public interface IRepository
    {
        Task<bool> Test();
        Task Reconnect(bool force = false);
        string Id { get; }
    }
}
