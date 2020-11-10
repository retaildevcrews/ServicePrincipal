using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CSE.Automation.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum ObjectClassification
    {
        Microsoft,
        External,
        Company
    }
}
