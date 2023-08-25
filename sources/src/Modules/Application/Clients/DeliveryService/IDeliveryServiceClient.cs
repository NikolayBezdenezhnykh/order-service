using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Clients.DeliveryService
{
    public interface IDeliveryServiceClient
    {
        Task<ReserveDeliveryResponse> ReserveAsync(string accessToken, ReserveDeliveryRequest request);

        Task CancelledAsync(string accessToken, CancellReserveDeliveryRequest request);
    }
}
