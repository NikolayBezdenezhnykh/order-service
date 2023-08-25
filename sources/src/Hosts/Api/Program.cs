using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Application.Extensions;
using Application.Behaviors.ResponseHandlers;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Application.Clients.ProductService;
using Application.Clients.AuthService;
using Application.Clients.DeliveryService;
using Application.Clients.StockService;
using Application.Clients.PaymentService;
using Application.KafkaMessageHandlers;
using Infrastructure.KafkaConsumer;
using Infrastructure.KafkaProducer;
using Application.Saga;
using Application.Clients.UserClient;

namespace order_service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddUserPostgreStorage(builder.Configuration);
            if (args.Length > 0 && args[0] == "update")
            {
                await UpdateDb(builder.Build());
                return;
            }

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddApiVersioning();
            builder.Services.AddHttpClient();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddMappings();
            builder.Services.AddMediatr();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<KafkaMessageInvoicePaidHandler>();
            builder.Services.AddSingleton<IKafkaMessageHandlerFactory, KafkaMessageHandlerFactory>();
            builder.Services.AddHostedService<KafkaConsumerHandler>();
            builder.Services.Configure<KafkaConsumerConfig>(options => builder.Configuration.GetSection("KafkaConsumer").Bind(options));
            builder.Services.AddScoped<IKafkaProducer, KafkaProducer>();
            builder.Services.Configure<KafkaProducerConfig>(options => builder.Configuration.GetSection("KafkaProducer").Bind(options));

            builder.Services.AddScoped<IOrderSagaManager, OrderSagaManager>();
            builder.Services.AddScoped<IOrderSagaStepHandler, OrderSagaStepHandler>();
            builder.Services.AddHostedService<OrderSagaBackgroundService>();
            builder.Services.Configure<AuthCredentialConfig>(options => builder.Configuration.GetSection("AuthCredential").Bind(options));
            builder.Services.AddSingleton(typeof(IIdempotentResponseHandlerFactory<>), typeof(IdempotentResponseHandlerFactory<>));
            builder.Services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetSection("ProductServiceUrl").Value);
            });
            builder.Services.AddHttpClient<IAuthServiceClient, AuthServiceClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetSection("AuthServiceUrl").Value);
            });
            builder.Services.AddHttpClient<IDeliveryServiceClient, DeliveryServiceClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetSection("DeliveryServiceUrl").Value);
            });
            builder.Services.AddHttpClient<IStockServiceClient, StockServiceClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetSection("StockServiceUrl").Value);
            });
            builder.Services.AddHttpClient<IPaymentServiceClient, PaymentServiceClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetSection("PaymentServiceUrl").Value);
            });
            builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetSection("UserServiceUrl").Value);
            });

            builder.Services.AddSingleton<IdempotentResponseLongHandler>();
            builder.Services.AddSwaggerGen(opt =>
            {
                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });
                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // адрес сервера auth-service
                    options.Authority = builder.Configuration.GetSection("IdentityServerClient:Authority").Value;

                    options.Audience = builder.Configuration.GetSection("IdentityServerClient:Audience").Value;

                    // сервер не поддерживает https
                    options.RequireHttpsMetadata = false;

                    if (builder.Environment.IsDevelopment())
                    {
                        options.TokenValidationParameters.ValidateIssuer = false;
                    }

                    options.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(30);
                });

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static async Task UpdateDb(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            await db.Database.MigrateAsync();
        }
    }
}