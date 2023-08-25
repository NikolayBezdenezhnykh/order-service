using Application.Extensions;
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
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, long>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly OrderDbContext _orderDbContext;

        public DeleteProductCommandHandler(
            OrderDbContext orderDbContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _orderDbContext = orderDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<long> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
        {
            Order order = null;
            var currentUserId = _httpContextAccessor.GetUserId();

            if (command.OrderId.HasValue)
            {
                order = await _orderDbContext.Orders
                    .Include(e => e.Items).SingleOrDefaultAsync(o => o.Id == command.OrderId);
            }
            else
            {
                order = await _orderDbContext.Orders
                    .Include(e => e.Items).SingleOrDefaultAsync(o => o.UserId == currentUserId && o.Status == (int)OrderStatus.Draft);
            }

            Validate(order, currentUserId);
            DeleteProductToOrder(order, command);

            order.TotalSum = order.Items.Sum(i => i.UnitPrice * i.Quantity);
            await _orderDbContext.SaveChangesAsync(cancellationToken);

            return order.Id;
        }

        private void Validate(Order order, Guid userId)
        {
            if (order == null)
            {
                throw new HttpRequestException("Данного заказа не существует.", null, HttpStatusCode.BadRequest);
            }

            if (order.Status != (int)OrderStatus.Draft)
            {
                throw new HttpRequestException("Данный заказ нельзя редактировать.", null, HttpStatusCode.Forbidden);
            }

            if (order.UserId != userId)
            {
                throw new HttpRequestException("Текущему пользователю нельзя редактировать заказ.", null, HttpStatusCode.Forbidden);
            }
        }

        private void DeleteProductToOrder(Order order, DeleteProductCommand command)
        {
            var existedProduct = order.Items.SingleOrDefault(o => o.ProductId == command.ProductId);
            if (existedProduct != null)
            {
                if (existedProduct.Quantity <= (command.Quantity ?? 1))
                {
                    order.Items.Remove(existedProduct);
                }
                else
                {
                    existedProduct.Quantity -= command.Quantity ?? 1;
                }
            }
        }
    }
}
