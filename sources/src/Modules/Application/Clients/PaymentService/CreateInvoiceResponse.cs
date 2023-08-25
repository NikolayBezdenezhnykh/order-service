using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Clients.PaymentService
{
    public class CreateInvoiceResponse
    {
        public int Id { get; set; }

        public string PaymentLink { get; set;}
    }
}
