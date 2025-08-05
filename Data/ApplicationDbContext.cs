using InventorySolution.Models;
using InventorySolution.Models.CustomerView;
using InventorySolution.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace InventorySolution.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>

    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        
        
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Add any custom model configurations here

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Location)
                    .WithMany(l => l.Orders)
                    .HasForeignKey(o => o.LocationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(o => o.Items)
                    .WithOne(i => i.Order)
                    .HasForeignKey(i => i.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        
        }
        
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> Items { get; set; }
        public DbSet<Category> Categories { get; set; } 
        public  DbSet<UnitMeasure> UnitMeasures { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<RecentlyViewed> RecentlyViewed { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ProductBatch> ProductBatches { get; set; }



    }
}
