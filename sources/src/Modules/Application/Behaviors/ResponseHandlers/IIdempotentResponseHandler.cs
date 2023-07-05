using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Behaviors.ResponseHandlers
{
    public interface IIdempotentResponseHandler<TResponse>
    {
        public string SerializeResult(TResponse response);

        public TResponse DeserializeResult(string response);
    }
}
