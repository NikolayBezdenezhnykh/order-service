using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Saga
{
    public static class SagaConstants
    {
        public const string ReserveProductStep = "ReserveProduct";
        public const string ReserveDeliveryStep = "ReserveDelivery";
        public const string GenerateLinkPaymentStep = "GenerateLinkPayment";
        public const string CancelReserveProductStep = "CancelReserveProduct";
        public const string CancelReserveDeliveryStep = "CancelReserveDelivery";
        public const string AnnulLinkPaymentStep = "AnnulLinkPayment";
    }
}
