using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public sealed class OrderSaga
    {
        public OrderSaga()
        {
            OrderSagaSteps = new List<OrderSagaStep>();
        }

        public int Id { get; set; }

        public Order Order { get; set; }

        public DateTime? StartNextDate { get; set; }

        public int? Status { get; set; }

        public int? RemainingAttemptsCount { get; set; }

        public DateTime? CreateDateAt { get; set; }

        public DateTime? UpdateDateAt { get; set; }
        
        public ICollection<OrderSagaStep> OrderSagaSteps { get; set; }

        public int? ReserveProductId { get; set; }

        public int? ReserveDeliveryId { get; set; }

        public int? InvoiceId { get; set; }

        public string PaymentLink { get; set; }
    }
}
