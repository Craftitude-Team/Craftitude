using System.Collections.Generic;
using Newtonsoft.Json;

namespace Craftitude.Authentication
{
    public class AuthenticationData
    {
        [JsonProperty("accessToken")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string AccessToken { get; private set; }

        [JsonProperty("clientToken")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string ClientToken { get; private set; }

        [JsonProperty("availableProfiles")]
        public List<Profile> AvailableProfiles { get; private set; }

        [JsonProperty("selectedProfile")]
        public Profile SelectedProfile { get; private set; }

        public AuthenticationData()
        {
            SelectedProfile = new Profile();
            AvailableProfiles = new List<Profile>();
        }
    }
}