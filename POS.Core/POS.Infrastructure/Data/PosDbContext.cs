using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data;

public class PosDbContext : DbContext
{
    public PosDbContext(DbContextOptions<PosDbContext> options) : base(options) { }

    public DbSet<TaxProfile> TaxProfiles { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Uom> Uoms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TaxProfiles
        var taxProfile = modelBuilder.Entity<TaxProfile>();
        taxProfile.ToTable("TaxProfiles");
        taxProfile.HasKey(t => t.TaxProfileId);
        taxProfile.Property(t => t.TaxProfileId).ValueGeneratedOnAdd();
        taxProfile.Property(t => t.Name).IsRequired().HasMaxLength(100);
        taxProfile.Property(t => t.CGST).HasPrecision(5, 2);
        taxProfile.Property(t => t.SGST).HasPrecision(5, 2);
        taxProfile.Property(t => t.IGST).HasPrecision(5, 2);
        taxProfile.Property(t => t.Cess).HasPrecision(5, 2);
        taxProfile.Property(t => t.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        taxProfile.Property(t => t.UpdatedAt).HasColumnType("datetime").ValueGeneratedOnAddOrUpdate();

        // Brands
        var brand = modelBuilder.Entity<Brand>();
        brand.ToTable("Brands");
        brand.HasKey(b => b.BrandId);
        brand.Property(b => b.BrandId).ValueGeneratedOnAdd();
        brand.Property(b => b.Name).IsRequired().HasMaxLength(150);
        brand.Property(b => b.Code).HasMaxLength(50);
        brand.Property(b => b.Slug).HasMaxLength(150);
        brand.Property(b => b.Description).HasMaxLength(500);
        brand.Property(b => b.ImageUrl).HasMaxLength(500);
        brand.Property(b => b.CreatedBy).HasMaxLength(50);
        brand.Property(b => b.UpdatedBy).HasMaxLength(50);
        brand.Property(b => b.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        brand.Property(b => b.UpdatedAt).HasColumnType("datetime").ValueGeneratedOnAddOrUpdate();
        brand.HasIndex(b => b.Name).IsUnique().HasDatabaseName("UK_Brand_Name");
        brand.HasIndex(b => b.Code).IsUnique().HasDatabaseName("UK_Brand_Code");
        modelBuilder.Entity<Brand>().HasQueryFilter(b => b.IsActive);

        // Categories
        var category = modelBuilder.Entity<Category>();
        category.ToTable("Categories");
        category.HasKey(c => c.CategoryId);
        category.Property(c => c.CategoryId).ValueGeneratedOnAdd();
        category.Property(c => c.Name).IsRequired().HasMaxLength(150);
        category.Property(c => c.ParentCategoryId);
        category.HasOne(c => c.ParentCategory)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        category.Property(c => c.Code).HasMaxLength(50);
        category.Property(c => c.Slug).HasMaxLength(150);
        category.Property(c => c.Description).HasMaxLength(500);
        category.Property(c => c.Level);
        category.Property(c => c.HSNCode).HasMaxLength(20);
        category.Property(c => c.DisplayOrder);
        category.Property(c => c.CreatedBy).HasMaxLength(50);
        category.Property(c => c.UpdatedBy).HasMaxLength(50);
        category.Property(c => c.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        category.Property(c => c.UpdatedAt).HasColumnType("datetime").ValueGeneratedOnAddOrUpdate();
        category.HasIndex(c => c.ParentCategoryId).HasDatabaseName("IX_Category_Parent");
        category.HasIndex(c => c.Code).IsUnique().HasDatabaseName("UK_Category_Code");
        modelBuilder.Entity<Category>().HasQueryFilter(c => c.IsActive);

        // Products
        var product = modelBuilder.Entity<Product>();
        product.ToTable("Products");
        product.HasKey(p => p.ProductId);
        product.Property(p => p.ProductId).ValueGeneratedOnAdd();
        product.Property(p => p.Name).IsRequired().HasMaxLength(200);
        product.Property(p => p.SKU).IsRequired().HasMaxLength(100);
        product.Property(p => p.Barcode).HasMaxLength(100);
        product.Property(p => p.Description).HasMaxLength(500);
        product.Property(p => p.CategoryId).IsRequired();
        product.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        product.Property(p => p.BrandId);
        product.HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.SetNull);
        product.Property(p => p.TaxProfileId).IsRequired();
        product.HasOne(p => p.TaxProfile)
                .WithMany()
                .HasForeignKey(p => p.TaxProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        product.Property(p => p.Unit).IsRequired().HasMaxLength(50);
        product.Property(p => p.CostPrice).HasPrecision(12, 2);
        product.Property(p => p.SellingPrice).HasPrecision(12, 2);
        product.Property(p => p.MRP).HasPrecision(12, 2);
        product.Property(p => p.HSNCode).HasMaxLength(20);
        product.Property(p => p.CreatedBy).HasMaxLength(50);
        product.Property(p => p.UpdatedBy).HasMaxLength(50);
        product.Property(p => p.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        product.Property(p => p.UpdatedAt).HasColumnType("datetime").ValueGeneratedOnAddOrUpdate();
        product.Property(p => p.RowVersion)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        product.HasIndex(p => p.SKU).IsUnique().HasDatabaseName("UK_Product_SKU");
        product.HasIndex(p => p.Barcode).IsUnique().HasDatabaseName("UK_Product_Barcode");
        product.HasIndex(p => p.Name).HasDatabaseName("IX_Product_Name");
        product.HasIndex(p => p.CategoryId).HasDatabaseName("IX_Product_Category");
        modelBuilder.Entity<Product>().HasQueryFilter(p => p.IsActive);

        // Customers
        var customer = modelBuilder.Entity<Customer>();
        customer.ToTable("Customers");
        customer.HasKey(c => c.CustomerId);
        customer.Property(c => c.CustomerId).HasMaxLength(36);
        customer.Property(c => c.FirstName).HasMaxLength(100);
        customer.Property(c => c.LastName).HasMaxLength(100);
        customer.Property(c => c.Phone).HasMaxLength(20);
        customer.Property(c => c.Email).HasMaxLength(256);
        customer.Property(c => c.Address).HasMaxLength(500);
        customer.Property(c => c.LoyaltyNumber).HasMaxLength(50);
        customer.Property(c => c.DateOfBirth).HasColumnType("date");
        customer.Property(c => c.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        customer.Property(c => c.UpdatedAt).HasColumnType("datetime").ValueGeneratedOnAddOrUpdate();
        customer.Property(c => c.RowVersion)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        customer.HasIndex(c => c.Phone).IsUnique().HasDatabaseName("UK_Customer_Phone");
        customer.HasIndex(c => c.Email).IsUnique().HasDatabaseName("UK_Customer_Email");
        modelBuilder.Entity<Customer>().HasQueryFilter(c => c.IsActive);

        // Uoms (unchanged - still Guid-based)
        var uom = modelBuilder.Entity<Uom>();
        uom.ToTable("Uoms");
        uom.HasKey(u => u.Id);
        uom.Property(u => u.Id).HasColumnName("UomId");
        uom.Ignore(u => u.UomId);
        uom.Property(u => u.Name).IsRequired().HasMaxLength(128);
        uom.Property(u => u.Code).IsRequired().HasMaxLength(32);
        uom.Property(u => u.Symbol).HasMaxLength(16);
        uom.Property(u => u.DecimalPlaces);
        uom.Property(u => u.Description).HasMaxLength(512);
        uom.Property(u => u.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        uom.Property(u => u.UpdatedAt).HasColumnType("datetime");
        uom.Property(u => u.RowVersion).HasColumnType("timestamp").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();
        uom.Property(u => u.IsActive);
        uom.HasIndex(u => u.Name).HasDatabaseName("IX_Uoms_Name");
        uom.HasIndex(u => u.Code).IsUnique().HasDatabaseName("UX_Uoms_Code");
        modelBuilder.Entity<Uom>().HasQueryFilter(u => u.IsActive);
    }
}
