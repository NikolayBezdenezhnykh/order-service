using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Behaviors.ResponseHandlers
{
    public interface IIdempotentResponseHandlerFactory<TResponse>
    {
        IIdempotentResponseHandler<TResponse> CreateIdempotentResponseHandler(Type responseType);
    }
}
