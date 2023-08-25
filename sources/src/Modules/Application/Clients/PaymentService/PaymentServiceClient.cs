using Api.Dtos;
using Application.Clients.StockService;
using Application.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Application.Clients.PaymentService
{
    public class PaymentServiceClient : IPaymentServiceClient
    {
        private static string _baseUrl = "api/v1.0/invoice";
        private readonly HttpClient _httpClient;

        public PaymentServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CreateInvoiceResponse> CreateInvoiceAsync(string accessToken, CreateInvoiceRequest request)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.PostAsJsonAsync(_baseUrl, request);
            response.EnsureSuccessStatusCode();

            var data = await response.GetTextResponseAsync();

            return JsonConvert.DeserializeObject<CreateInvoiceResponse>(data);
        }

        public async Task CancellInvoiceAsync(string accessToken, CancellInvoiceRequest request)
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

            var response = await _httpClient.PutAsync(_baseUrl + "/cancelled/" + request.Id, null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<InvoiceDto> GetInvoiceAsync(string accessToken, int invoiceId)
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

            var response = await _httpClient.GetFromJsonAsync<InvoiceDto>(_baseUrl + "/" + invoiceId);
            return response;
        }
    }
}
