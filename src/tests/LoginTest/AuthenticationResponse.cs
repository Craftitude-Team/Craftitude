using Newtonsoft.Json;

namespace Craftitude.Authentication
{
    public class AuthenticationResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("json_data")]
        public AuthenticationData Data { get; set; }

        public AuthenticationResponse()
        {
            Data = new AuthenticationData();
        }
    }
}