using Microsoft.EntityFrameworkCore;
using POS.AuthService.Entities;

namespace POS.AuthService.Infrastructure
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<Store> Stores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var store = modelBuilder.Entity<Store>();
            store.ToTable("Stores");
            store.HasKey(s => s.StoreCode);
            store.Property(s => s.StoreCode).ValueGeneratedNever();
            store.Property(s => s.StoreName).IsRequired().HasMaxLength(200);
            store.Property(s => s.Address).HasMaxLength(500);
            store.Property(s => s.TaxId).HasMaxLength(20);
            store.Property(s => s.IsActive).HasDefaultValue(true);
        }
    }
}
