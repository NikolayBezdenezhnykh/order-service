using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Clients.AuthService
{
    public class AuthResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
