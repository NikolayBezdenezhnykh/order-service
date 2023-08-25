using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Saga
{
    public interface IOrderSagaStepHandler
    {
        Task HandleStepAsync(OrderSaga orderSaga);

        Task HandleCompensationStepAsync(OrderSaga orderSaga);
    }
}
