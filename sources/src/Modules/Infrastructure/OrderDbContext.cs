using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class OrderDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderSaga> OrderSaga { get; set; }

        public DbSet<ProcessedMessage> ProcessedMessages { get; set; }

        public OrderDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp with time zone");
        }
    }
}
