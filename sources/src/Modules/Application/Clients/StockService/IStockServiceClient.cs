using Api.Dtos;
using Application.Clients.DeliveryService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Clients.StockService
{
    public interface IStockServiceClient
    {
        Task<ReserveProductResponse> ReserveAsync(string accessToken, ReserveProductRequest request);

        Task CancelledAsync(string accessToken, CancellReserveProductRequest request);
    }
}
