// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CSE.Automation.Model
{
    public abstract class QueueMessage
    {
        public QueueMessageType QueueMessageType { get; set; }

        public int Attempt { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum QueueMessageType
    {
        Data,
        Audit,
    }
}
