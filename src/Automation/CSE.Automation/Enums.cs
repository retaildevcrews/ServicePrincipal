// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DALCollection
    {
        Audit,
        ObjectTracking,
        ProcessorConfiguration,
    }

    [JsonConverter(typeof(StringEnumConverter))]

    public enum ProcessorType
    {
        ServicePrincipal,
        User,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TypeFilter
    {
        Any,
        ServicePrincipal,
        User,
        Application,
        Configuration,
        Audit,
    }
}
