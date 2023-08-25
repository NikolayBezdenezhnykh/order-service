using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class OrderPaidMessage
    {
        public string EmailTo { get; set; }

        public string Template => "OrderPaid";

        public Dictionary<string, string>  DynamicParams { get; set; }
    }
}
