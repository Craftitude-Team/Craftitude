using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Craftitude
{
    [Serializable]
    public class SetupStep
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("arguments")]
        public IEnumerable<string> Arguments { get; set; }
    }
}