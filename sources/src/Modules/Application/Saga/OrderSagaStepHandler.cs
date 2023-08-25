using Api.Dtos;
using Application.Clients.AuthService;
using Application.Clients.DeliveryService;
using Application.Clients.PaymentService;
using Application.Clients.StockService;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Saga
{
    public class OrderSagaStepHandler : IOrderSagaStepHandler
    {
        private readonly IAuthServiceClient _authServiceClient;
        private readonly IStockServiceClient _stockServiceClient;
        private readonly IDeliveryServiceClient _deliveryServiceClient;
        private readonly IPaymentServiceClient _paymentServiceClient;

        public OrderSagaStepHandler(
            IAuthServiceClient authServiceClient,
            IStockServiceClient stockServiceClient,
            IDeliveryServiceClient deliveryServiceClient,
            IPaymentServiceClient paymentServiceClient)
        {
            _authServiceClient = authServiceClient;
            _stockServiceClient = stockServiceClient;
            _deliveryServiceClient = deliveryServiceClient;
            _paymentServiceClient = paymentServiceClient;
        }

        public async Task HandleStepAsync(OrderSaga orderSaga)
        {
            var responseAuth = await _authServiceClient.GetTokenAsync(new List<string>() { "stock-service", "delivery-service", "payment-service" });

            foreach (var orderStep in orderSaga.OrderSagaSteps.Where(s => s.Status != (int)OrderSagaStatus.Done && s.IsCompensation != true))
            {
                if (orderStep.Name == SagaConstants.ReserveProductStep)
                {
                    var request = new ReserveProductRequest()
                    {
                        Products = orderSaga.Order.Items.Select(s => new ProductDto() { ProductId = s.ProductId, Quantity = s.Quantity }).ToList()
                    };
                    await TryPerfomRequestAsync(async () =>
                    {
                        var responseStock = await _stockServiceClient.ReserveAsync(responseAuth.AccessToken, request);
                        orderSaga.ReserveProductId = responseStock.Id;
                    }, orderStep);
                }

                if (orderStep.Name == SagaConstants.ReserveDeliveryStep)
                {
                    var request = new ReserveDeliveryRequest()
                    {
                        Address = orderSaga.Order.Delivery.Address,
                        Date = orderSaga.Order.Delivery.Date.Value
                    };
                    await TryPerfomRequestAsync(async () =>
                    {
                        var responseDelivery = await _deliveryServiceClient.ReserveAsync(responseAuth.AccessToken, request);
                        orderSaga.ReserveDeliveryId = responseDelivery.Id;
                    }, orderStep);
                }

                if (orderStep.Name == SagaConstants.GenerateLinkPaymentStep)
                {
                    var request = new CreateInvoiceRequest()
                    {
                        Sum = orderSaga.Order.TotalSum.Value
                    };

                    await TryPerfomRequestAsync(async () =>
                    {
                        var responseInvoice = await _paymentServiceClient.CreateInvoiceAsync(responseAuth.AccessToken, request);
                        orderSaga.InvoiceId = responseInvoice.Id;
                        orderSaga.PaymentLink = responseInvoice.PaymentLink;
                    }, orderStep);
                }
            }
        }

        public async Task HandleCompensationStepAsync(OrderSaga orderSaga)
        {
            var responseAuth = await _authServiceClient.GetTokenAsync(new List<string>() { "stock-service", "delivery-service", "payment-service" });

            foreach (var orderStep in orderSaga.OrderSagaSteps.Where(s => s.Status != (int)OrderSagaStatus.Done && s.IsCompensation == true))
            {
                if (orderStep.Name == SagaConstants.CancelReserveProductStep)
                {
                    var request = new CancellReserveProductRequest()
                    {
                        Id = orderSaga.ReserveProductId.Value,
                    };
                    await TryPerfomRequestAsync(async () =>
                    {
                        await _stockServiceClient.CancelledAsync(responseAuth.AccessToken, request);
                    }, orderStep);
                }

                if (orderStep.Name == SagaConstants.CancelReserveDeliveryStep)
                {
                    var request = new CancellReserveDeliveryRequest()
                    {
                        Id = orderSaga.ReserveDeliveryId.Value
                    };
                    await TryPerfomRequestAsync(async () =>
                    {
                        await _deliveryServiceClient.CancelledAsync(responseAuth.AccessToken, request);
                    }, orderStep);
                }

                if (orderStep.Name == SagaConstants.AnnulLinkPaymentStep)
                {
                    var request = new CancellInvoiceRequest()
                    {
                        Id = orderSaga.InvoiceId.Value,
                    };

                    await TryPerfomRequestAsync(async () =>
                    {
                        await _paymentServiceClient.CancellInvoiceAsync(responseAuth.AccessToken, request);
                    }, orderStep);
                }
            }
        }

        private async Task TryPerfomRequestAsync(Func<Task> func, OrderSagaStep orderSagaStep)
        {
            try
            {
                await func();
                orderSagaStep.Status = (int)OrderSagaStatus.Done;
            }
            catch
            {
                orderSagaStep.Status = (int)OrderSagaStatus.Error;
                throw;
            }
        }
    }
}
