using Newtonsoft.Json;

namespace Craftitude.Authentication
{
    public class AuthenticationRequest
    {
        [JsonProperty("agent")]
        public AuthenticationAgent Agent { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        public AuthenticationRequest()
        {
            Agent = new AuthenticationAgent();
        }
    }
}