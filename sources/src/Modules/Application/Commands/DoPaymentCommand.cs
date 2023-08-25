using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands
{
    public class DoPaymentCommand : IRequest<string>
    {
        public long? OrderId { get; set; }

        public DateTime? DateDelivery { get; set; }

        public string AddressDelivery { get; set; }
    }
}
