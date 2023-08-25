using Application.Clients.AuthService;
using Application.Clients.PaymentService;
using Confluent.Kafka;
using Domain;
using Infrastructure;
using Infrastructure.KafkaConsumer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Saga
{
    public class OrderSagaBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public OrderSagaBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var orderDbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                        var ordersSaga = await orderDbContext.OrderSaga
                            .Include(c => c.OrderSagaSteps)
                            .Include(c => c.Order).ThenInclude(i => i.Items)
                            .Include(c => c.Order).ThenInclude(i => i.Delivery)
                            .Where(s => (s.Status == (int)OrderSagaStatus.New || s.Status == (int)OrderSagaStatus.Error) && DateTime.UtcNow >= s.StartNextDate)
                            .ToListAsync();

                        foreach (var orderSaga in ordersSaga)
                        {
                            var orderSagaManager = scope.ServiceProvider.GetRequiredService<IOrderSagaManager>();
                            if (orderSaga.InvoiceId.HasValue)
                            {
                                var authServiceClient = scope.ServiceProvider.GetRequiredService<IAuthServiceClient>();
                                var paymentServiceClient = scope.ServiceProvider.GetRequiredService<IPaymentServiceClient>();

                                var responseAuth = await authServiceClient.GetTokenAsync(new List<string>() { "payment-service" });
                                var response = await paymentServiceClient.GetInvoiceAsync(responseAuth.AccessToken, orderSaga.InvoiceId.Value);

                                if(response.Status == "Paid")
                                {
                                    await orderSagaManager.PaidPaymentLinkAsync(orderSaga);
                                }
                                else
                                {
                                    await orderSagaManager.AnnulPaymentLinkAsync(orderSaga);
                                }
                            }
                            else
                            {
                                await orderSagaManager.AnnulPaymentLinkAsync(orderSaga);
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine($"OrderSagaBackgroundService error: {e.Message}");
                }

                await Task.Delay(1000, cancellationToken);
            }
        }
    }
}
