using Application.Commands;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class CreateOrderCommand : IRequest<long>, IIdempotentCommand
    {
        public string CommandType => "createOrder";

        public long? CustomerId { get; set; }

        public decimal? TotalSum { get; set; }

        public List<OrderItemDto> Items { get; set; }
    }
}
