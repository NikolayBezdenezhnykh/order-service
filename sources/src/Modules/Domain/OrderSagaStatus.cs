using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public enum OrderSagaStatus
    {
        New = 10,

        InProgress = 20,

        Done = 30,

        Error = 40,

        Failed = 50
    }
}
