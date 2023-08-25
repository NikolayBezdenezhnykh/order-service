using Application.Commands;
using Application.Dtos;
using Application.Extensions;
using AutoMapper;
using Domain;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Application.Queries
{
    public class OrderQueryHandler : IRequestHandler<OrderQuery, OrderDto>
    {
        private readonly IMapper _mapper;
        private readonly OrderDbContext _orderDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderQueryHandler(
            IMapper mapper,
            OrderDbContext orderDbContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _orderDbContext = orderDbContext;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<OrderDto> Handle(OrderQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderDbContext
                .Orders
                .AsNoTracking()
                .Include(c => c.Items)
                .Include(c => c.Delivery)
                .Where(x => x.Id == request.Id).SingleOrDefaultAsync(cancellationToken);

            Validate(order, _httpContextAccessor.GetUserId());

            return _mapper.Map<OrderDto>(order);
        }

        private void Validate(Order order, Guid userId)
        {
            if (order == null)
            {
                throw new HttpRequestException("Данного заказа не существует.", null, HttpStatusCode.BadRequest);
            }

            if (order.UserId != userId)
            {
                throw new HttpRequestException("У пользователя нет прав на просмотр текущего заказа.", null, HttpStatusCode.BadRequest);
            }
        }
    }
}
