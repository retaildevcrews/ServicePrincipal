// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation;

namespace CSE.Automation.Interfaces
{
    public interface IServiceResolver
    {
        T GetService<T>(string keyName);
    }
}
