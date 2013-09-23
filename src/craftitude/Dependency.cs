using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Craftitude
{
    [Serializable]
    public class Dependency
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public DependencyType Type { get; set; }

        [JsonProperty("versions")]
        public string Versions { get; set; }
    }
}