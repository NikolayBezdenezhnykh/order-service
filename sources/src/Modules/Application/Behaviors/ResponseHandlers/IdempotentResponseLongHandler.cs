using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Behaviors.ResponseHandlers
{
    public class IdempotentResponseLongHandler : IIdempotentResponseHandler<long>
    {
        public string SerializeResult(long response)
        {
            return response.ToString();
        }

        public long DeserializeResult(string response)
        {
            return long.Parse(response);
        }
    }
}
