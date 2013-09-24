using Newtonsoft.Json;

namespace Craftitude.Authentication
{
    public class Profile
    {
        [JsonProperty("id")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string Id { get; private set; }

        [JsonProperty("name")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string Name { get; private set; }
    }
}