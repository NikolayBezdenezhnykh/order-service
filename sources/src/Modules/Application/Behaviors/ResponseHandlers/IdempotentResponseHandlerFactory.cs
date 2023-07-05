using Microsoft.Extensions.DependencyInjection;

namespace Application.Behaviors.ResponseHandlers
{
    public class IdempotentResponseHandlerFactory<TResponse> : IIdempotentResponseHandlerFactory<TResponse>
    {
        private readonly IServiceProvider _serviceProvider;

        public IdempotentResponseHandlerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IIdempotentResponseHandler<TResponse> CreateIdempotentResponseHandler(Type responseType)
        {
            var responseHandler = responseType switch
            {
                Type _ when responseType == typeof(long) => _serviceProvider.GetRequiredService<IdempotentResponseLongHandler>(),
                _ => throw new NotImplementedException()
            };

            return responseHandler as IIdempotentResponseHandler<TResponse>;
        }
    }
}
