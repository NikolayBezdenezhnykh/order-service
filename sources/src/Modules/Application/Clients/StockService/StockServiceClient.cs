using Api.Dtos;
using Application.Clients.DeliveryService;
using Application.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Application.Clients.StockService
{
    public class StockServiceClient : IStockServiceClient
    {
        private static string _baseUrl = "api/v1.0/stock/reserve";
        private readonly HttpClient _httpClient;

        public StockServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ReserveProductResponse> ReserveAsync(string accessToken, ReserveProductRequest request)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.PostAsJsonAsync(_baseUrl, request);
            response.EnsureSuccessStatusCode();

            var data = await response.GetTextResponseAsync();

            return JsonConvert.DeserializeObject<ReserveProductResponse>(data);
        }

        public async Task CancelledAsync(string accessToken, CancellReserveProductRequest request)
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

            var response = await _httpClient.PutAsync(_baseUrl + "/cancelled/" + request.Id, null);
            response.EnsureSuccessStatusCode();
        }
    }
}
