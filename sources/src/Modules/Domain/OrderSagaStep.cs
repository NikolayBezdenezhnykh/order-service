using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public sealed class OrderSagaStep
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? Status { get; set; }

        public bool? IsCompensation { get; set; }
    }
}
