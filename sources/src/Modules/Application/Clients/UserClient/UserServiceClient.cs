using Azure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Application.Clients.UserClient
{
    public class UserServiceClient : IUserServiceClient
    {
        private static string _baseUrl = "api/v1.0/user";
        private readonly HttpClient _httpClient;

        public UserServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserDto> GetUserAsync(string accessToken, Guid userId)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetFromJsonAsync<UserDto>(_baseUrl + "/" + userId.ToString());
            return response;
        }
    }
}
