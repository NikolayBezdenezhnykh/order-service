using AutoMapper;
using Domain;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http;
using Application.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Clients.ProductService;
using System.Threading;
using System.Net;

namespace Application.Commands
{
    public class AddProductCommandHandler : IRequestHandler<AddProductCommand, long>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductServiceClient _productServiceClient;
        private readonly OrderDbContext _orderDbContext;

        public AddProductCommandHandler(
            OrderDbContext orderDbContext,
            IHttpContextAccessor httpContextAccessor,
            IProductServiceClient productServiceClient)
        {
            _orderDbContext = orderDbContext;
            _httpContextAccessor = httpContextAccessor;
            _productServiceClient = productServiceClient;
        }

        public async Task<long> Handle(AddProductCommand command, CancellationToken cancellationToken)
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
                if (order == null)
                {
                    order = new Order() { Status = (int)OrderStatus.Draft, UserId = currentUserId };
                    _orderDbContext.Orders.Add(order);
                }
            }

            Validate(order, currentUserId);

            var product = await _productServiceClient.GetProduct(command.ProductId.Value);
            AddProductToOrder(order, product, command.Quantity ?? 1);

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

        private void AddProductToOrder(Order order, ProductDto product, int quantity)
        {
            var existedProduct = order.Items.SingleOrDefault(o => o.ProductId == product.ProductId);
            if (existedProduct != null)
            {
                existedProduct.UnitPrice = product.Price;
                existedProduct.Quantity += quantity;
                existedProduct.ProductName = product.ProductName;

                return;
            }

            var orderItem = new OrderItem()
            {
                ProductId = product.ProductId,
                Quantity = quantity,
                UnitPrice = product.Price,
                ProductName = product.ProductName,
            };

            order.Items.Add(orderItem);
        }
    }
}
