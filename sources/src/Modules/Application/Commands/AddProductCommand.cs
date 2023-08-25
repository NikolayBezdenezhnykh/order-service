using Application.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands
{
    public class AddProductCommand : IRequest<long>, IIdempotentCommand
    {
        public string CommandType => "addProduct";

        public long? OrderId { get; set; }

        public long? ProductId { get; set; }

        public int? Quantity { get; set; }
    }
}
