using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Clients.ProductService
{
    public interface IProductServiceClient
    {
        Task<IReadOnlyList<ProductDto>> GetProducts(IReadOnlyList<long> productIds);

        Task<ProductDto> GetProduct(long productId);
    }
}
