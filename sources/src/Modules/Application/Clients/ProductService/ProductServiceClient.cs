using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Application.Clients.ProductService
{
    public class ProductServiceClient : IProductServiceClient
    {
        private static string _baseUrl = "api/v1.0/product";
        private readonly HttpClient _httpClient;

        public ProductServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProductDto> GetProduct(long productId)
        {
            return (await GetProducts(new List<long>() { productId })).SingleOrDefault();
        }

        public async Task<IReadOnlyList<ProductDto>> GetProducts(IReadOnlyList<long> productIds)
        {
            var url = _baseUrl + AddQueryParameters(productIds);
            var response = await _httpClient.GetFromJsonAsync<IReadOnlyList<ProductDto>>(url);
            return response;
        }
        
        private string AddQueryParameters(IReadOnlyList<long> productIds)
        {
            var query = new StringBuilder("?");
            foreach (var productId in productIds) 
            {
                query.Append($"ids={productId}&");
            }

            return query.ToString();
        }
    }
}
