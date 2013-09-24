using Newtonsoft.Json;

namespace Craftitude.Authentication
{
    public class AuthenticationAgent
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("version")]
        public int Version { get; set; }
    }
}