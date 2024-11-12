using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pinewood.Api.Models;
using Pinewood.Api.Models.HealthCheck;

namespace Pinewood.Api.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Healthcheck> Healthcheck { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var customerList = new List<Customer>
            {
                new Customer { Id = 1, Name = "John Doe", Email = "john.doe@test.com", Phone = "07917195800" },
                new Customer { Id = 2, Name = "Jane Doe", Email = "jane.doe@test.com", Phone = "07917195801" },
                new Customer { Id = 3, Name = "John Smith", Email = "john.smith@test.com", Phone = "07917195802" },
            };
            modelBuilder.Entity<Customer>().HasData(customerList);
        }
    };
}
