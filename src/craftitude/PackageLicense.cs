using System;
using Newtonsoft.Json;
using YamlDotNet.RepresentationModel.Serialization;

namespace Craftitude
{
    [Serializable]
    public class PackageLicense
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }
        
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("text-url")]
        [YamlAlias("Text-Url")]
        public string TextUrl { get; set; }
    }
}