using Application.Behaviors;
using Application.Commands;
using Application.Dtos;
using Application.Mappings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Extensions
{
    public static class MediatrHostingExtensions
    {
        public static IServiceCollection AddMediatr(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining(typeof(AddProductCommand)));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining(typeof(DeleteProductCommand)));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining(typeof(DoPaymentCommand)));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(IdempotentBehavior<,>));

            return services;
        }
    }
}
