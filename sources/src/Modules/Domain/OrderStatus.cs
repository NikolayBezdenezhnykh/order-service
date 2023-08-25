using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public enum OrderStatus
    {
        Draft = 10,

        WaitingPay = 20,

        Paid = 30,

        Canceled = 40
    }
}
