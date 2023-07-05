using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Application.Extensions;
using Azure;
using Application.Behaviors.ResponseHandlers;

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
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddMappings();
            builder.Services.AddMediatr();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton(typeof(IIdempotentResponseHandlerFactory<>), typeof(IdempotentResponseHandlerFactory<>));
            builder.Services.AddSingleton<IdempotentResponseLongHandler>();

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthentication();

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