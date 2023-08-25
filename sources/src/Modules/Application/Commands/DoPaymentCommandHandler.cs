using Application.Extensions;
using Application.Saga;
using Domain;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands
{
    public class DoPaymentCommandHandler : IRequestHandler<DoPaymentCommand, string>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrderSagaManager _orderSagaManager;
        private readonly OrderDbContext _orderDbContext;

        public DoPaymentCommandHandler(
            OrderDbContext orderDbContext,
            IHttpContextAccessor httpContextAccessor,
            IOrderSagaManager orderSagaManager)
        {
            _orderDbContext = orderDbContext;
            _httpContextAccessor = httpContextAccessor;
            _orderSagaManager = orderSagaManager;
        }

        public async Task<string> Handle(DoPaymentCommand command, CancellationToken cancellationToken)
        {
            var currentUserId = _httpContextAccessor.GetUserId();
            var order = await _orderDbContext.Orders
                    .Include(e => e.Items)
                    .SingleOrDefaultAsync(o => o.Id == command.OrderId);

            Validate(order, currentUserId);

            if (order.Status == (int)OrderStatus.WaitingPay)
            {
                return await _orderSagaManager.GetPaymentLinkAsync(order);
            }
            else if (order.Status == (int)OrderStatus.Paid || order.Status == (int)OrderStatus.Canceled)
            {
                throw new HttpRequestException("Данный заказ невозможно оплатить.", null, HttpStatusCode.Forbidden);
            }

            order.Delivery = new Delivery()
            {
                Address = command.AddressDelivery,
                Date = command.DateDelivery
            };

            var paymentLink = await _orderSagaManager.GeneratePaymentLinkAsync(order);           

            return paymentLink;
        }

        private void Validate(Order order, Guid userId)
        {
            if (order == null)
            {
                throw new HttpRequestException("Данного заказа не существует.", null, HttpStatusCode.BadRequest);
            }

            if (order.UserId != userId)
            {
                throw new HttpRequestException("Текущему пользователю нельзя оплатить данный заказ.", null, HttpStatusCode.Forbidden);
            }
        }
    }
}
