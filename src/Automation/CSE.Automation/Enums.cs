using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CSE.Automation
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DALCollection { Audit, ObjectTracking, ProcessorConfiguration };

    [JsonConverter(typeof(StringEnumConverter))]

    public enum ProcessorType { ServicePrincipal, User };

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TypeFilter { any, servicePrincipal, user, application, configuration, audit };


}
