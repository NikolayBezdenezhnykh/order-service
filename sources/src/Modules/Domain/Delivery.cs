using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public sealed class Delivery
    {
        public long Id { get; set; }

        public DateTime? Date { get; set; }

        public string Address { get; set; }
    }
}
