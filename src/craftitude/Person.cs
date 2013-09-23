using System;
using Newtonsoft.Json;
using YamlDotNet.RepresentationModel.Serialization;

namespace Craftitude
{
    [Serializable]
    public class Person
    {
        [JsonProperty("username", Required = Required.Always)]
        public string Username { get; set; }

        [JsonProperty("realname")]
        public string Realname { get; set; }

        [JsonProperty("email")]
        [YamlAlias("E-Mail")]
        public string Email { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}