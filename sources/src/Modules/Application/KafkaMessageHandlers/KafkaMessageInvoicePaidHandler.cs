using Application.Clients.PaymentService;
using Application.Dtos;
using Application.Saga;
using Domain;
using Infrastructure;
using Infrastructure.KafkaConsumer;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.KafkaMessageHandlers
{
    public class KafkaMessageInvoicePaidHandler : IKafkaMessageHandler
    {
        private readonly IOrderSagaManager _orderSagaManager;
        private readonly OrderDbContext _orderDbContext;

        public KafkaMessageInvoicePaidHandler(
            OrderDbContext orderDbContext,
            IOrderSagaManager orderSagaManager)
        {
            _orderDbContext = orderDbContext;
            _orderSagaManager = orderSagaManager;
        }

        public async Task HandleMessageAsync(string message)
        {
            var invoiceDto = JsonConvert.DeserializeObject<InvoiceDto>(message);

            var orderSaga = await _orderDbContext.OrderSaga
                .Include(c => c.Order).ThenInclude(i => i.Items)
                .Include(c => c.Order).ThenInclude(i => i.Delivery)
                .SingleOrDefaultAsync(u => u.InvoiceId == invoiceDto.Id);
            if (orderSaga == null)
            {
                return;
            }

            await _orderSagaManager.PaidPaymentLinkAsync(orderSaga);

        }
    }
}
