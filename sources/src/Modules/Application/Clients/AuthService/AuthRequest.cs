using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Clients.AuthService
{
    public class AuthRequest
    {
        [JsonProperty("grant_type")]
        public string GrantType { get; } = "client_credentials";

        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        [JsonProperty("resource")]
        public string[] Resource { get; set; }

    }
}
