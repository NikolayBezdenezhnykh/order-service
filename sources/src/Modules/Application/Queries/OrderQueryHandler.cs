using Application.Dtos;
using AutoMapper;
using Domain;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace Application.Queries
{
    public class OrderQueryHandler : IRequestHandler<OrderQuery, OrderDto>
    {
        private readonly IMapper _mapper;
        private readonly OrderDbContext _orderDbContext;

        public OrderQueryHandler(
            IMapper mapper,
            OrderDbContext orderDbContext)
        {
            _orderDbContext = orderDbContext;
            _mapper = mapper;
        }

        public async Task<OrderDto> Handle(OrderQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderDbContext
                .Orders
                .AsNoTracking()
                .Include(c => c.Items)
                .Where(x => x.Id == request.Id).FirstOrDefaultAsync(cancellationToken);

            if (order == null)
            {
                return null;
            }

            return _mapper.Map<OrderDto>(order);
        }
    }
}
