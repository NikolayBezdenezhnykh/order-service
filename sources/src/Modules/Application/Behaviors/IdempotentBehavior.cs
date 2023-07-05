using Application.Behaviors.ResponseHandlers;
using Application.Commands;
using Domain;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Behaviors
{
    internal class IdempotentBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
        where TRequest : IIdempotentCommand
    {
        private const string _idempotencyKeyHeader = "Idempotency-Key";
        private readonly IServiceProvider _serviceProvider;

        public IdempotentBehavior(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not IIdempotentCommand)
            {
                return await next();
            }

            var httpContextAccessor = _serviceProvider.GetRequiredService<IHttpContextAccessor>();
            
            if (!httpContextAccessor.HttpContext.Request.Headers.TryGetValue(_idempotencyKeyHeader, out var idempotencyKeyStrValue)
                || !Guid.TryParse(idempotencyKeyStrValue.ToString(), out var idempotencyKey))
            {
                return await next();
            }

            var idempotentResponseHandlerFactory = _serviceProvider.GetRequiredService<IIdempotentResponseHandlerFactory<TResponse>>();
            var responseHandler = idempotentResponseHandlerFactory.CreateIdempotentResponseHandler(typeof(TResponse));

            using var scope = _serviceProvider.CreateScope();

            var orderDbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            using var transaction = await orderDbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var response = await next();

                var processedMessage = new ProcessedMessage()
                {
                    Id = idempotencyKey,
                    Result = responseHandler.SerializeResult(response),
                    Scope = request.CommandType,
                    TimeStamp = DateTime.Now
                };

                orderDbContext.ProcessedMessages.Add(processedMessage);
                await orderDbContext.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                var (isSuccess, response) =  await TryGetExistedResponseAsync(idempotencyKey, responseHandler, cancellationToken);
                if (isSuccess)
                {
                    return response;
                }

                throw;
            }
        }

        private async Task<(bool isSuccess, TResponse response)> TryGetExistedResponseAsync(Guid idempotencyKey, IIdempotentResponseHandler<TResponse> responseHandler, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var orderDbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

                var processedMessage = await orderDbContext.ProcessedMessages.AsNoTracking().SingleAsync(p => p.Id == idempotencyKey, cancellationToken);

                return (isSuccess: true, response: responseHandler.DeserializeResult(processedMessage.Result));
            }
            catch
            {
                return (isSuccess: false, response: default);
            }
        }
    }
}
