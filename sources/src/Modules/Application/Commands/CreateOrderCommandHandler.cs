using Application.Dtos;
using AutoMapper;
using Domain;
using Infrastructure;
using MediatR;

namespace Application.Commands
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, long>
    {
        private readonly IMapper _mapper;
        private readonly OrderDbContext _orderDbContext;

        public CreateOrderCommandHandler(
            IMapper mapper,
            OrderDbContext orderDbContext)
        {
            _orderDbContext = orderDbContext;
            _mapper = mapper;
        }

        public async Task<long> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            var order = _mapper.Map<Order>(command);

            _orderDbContext.Orders.Add(order);

            await _orderDbContext.SaveChangesAsync(cancellationToken);

            return order.Id;
        }
    }
}
