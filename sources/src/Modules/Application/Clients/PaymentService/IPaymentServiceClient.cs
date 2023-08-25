using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Clients.PaymentService
{
    public interface IPaymentServiceClient
    {
        Task<CreateInvoiceResponse> CreateInvoiceAsync(string accessToken, CreateInvoiceRequest request);

        Task CancellInvoiceAsync(string accessToken, CancellInvoiceRequest request);

        Task<InvoiceDto> GetInvoiceAsync(string accessToken, int invoiceId);
    }
}
