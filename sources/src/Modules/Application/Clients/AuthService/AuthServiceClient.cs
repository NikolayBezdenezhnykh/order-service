using Application.Clients.ProductService;
using Application.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Application.Clients.AuthService
{
    public class AuthServiceClient : IAuthServiceClient
    {
        private static string _baseUrl = "api/v1.0/auth/token";
        private readonly HttpClient _httpClient;
        private readonly AuthCredentialConfig _options;

        public AuthServiceClient(
            HttpClient httpClient,
            IOptions<AuthCredentialConfig> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<AuthResponse> GetTokenAsync(IReadOnlyList<string> resources)
        {
            var req = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", _options.ClientId),
                new KeyValuePair<string, string>("client_secret", _options.ClientSecret)
            };

            foreach(var resource in resources)
            {
                req.Add(new KeyValuePair<string, string>("resource", resource));
            }

            var response = await _httpClient.PostAsync(_baseUrl, new FormUrlEncodedContent(req));
            response.EnsureSuccessStatusCode();

            var data = await response.GetTextResponseAsync();

            return JsonConvert.DeserializeObject<AuthResponse>(data);
        }
    }
}
