using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Application.Saga
{
    public interface IOrderSagaManager
    {
        Task<string> GeneratePaymentLinkAsync(Order order);

        Task AnnulPaymentLinkAsync(OrderSaga orderSaga);

        Task PaidPaymentLinkAsync(OrderSaga orderSaga);

        Task<string> GetPaymentLinkAsync(Order order);
    }
}
