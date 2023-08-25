using Application.Clients.AuthService;
using Application.Clients.ProductService;
using Application.Extensions;
using Azure.Core;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Application.Clients.DeliveryService
{
    public class DeliveryServiceClient : IDeliveryServiceClient
    {
        private static string _baseUrl = "api/v1.0/delivery/reserve";
        private readonly HttpClient _httpClient;

        public DeliveryServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ReserveDeliveryResponse> ReserveAsync(string accessToken, ReserveDeliveryRequest request)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.PostAsJsonAsync(_baseUrl, request);
            response.EnsureSuccessStatusCode();

            var data = await response.GetTextResponseAsync();

            return JsonConvert.DeserializeObject<ReserveDeliveryResponse>(data);
        }

        public async Task CancelledAsync(string accessToken, CancellReserveDeliveryRequest request)
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

            var response = await _httpClient.PutAsync(_baseUrl + "/cancelled/" + request.Id, null);
            response.EnsureSuccessStatusCode();
        }
    }
}
