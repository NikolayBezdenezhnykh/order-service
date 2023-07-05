using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public sealed class Order
    {
        public Order() 
        {
            Items = new List<OrderItem>();
        }

        public long Id { get; set; }

        public long? CustomerId { get; set; }

        public DateTime? OrderDate { get; set; }

        public int? Status { get; set; }

        public decimal? TotalSum { get; set; }

        public List<OrderItem> Items { get; set; }
    }
}
