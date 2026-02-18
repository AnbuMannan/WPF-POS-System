using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Domain.Enums;

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
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    public DbSet<PurchaseEntry> PurchaseEntries { get; set; }
    public DbSet<PurchaseEntryItem> PurchaseEntryItems { get; set; }
    public DbSet<PurchaseReturn> PurchaseReturns { get; set; }
    public DbSet<PurchaseReturnItem> PurchaseReturnItems { get; set; }
    public DbSet<Batch> Batches { get; set; }
    public DbSet<StockLedgerEntry> StockLedgerEntries { get; set; }
    public DbSet<SupplierPayment> SupplierPayments { get; set; }
    public DbSet<SupplierTransaction> SupplierTransactions { get; set; }
    public DbSet<StockAdjustment> StockAdjustments { get; set; }
    public DbSet<StockAdjustmentItem> StockAdjustmentItems { get; set; }

    // Billing module
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Voucher> Vouchers { get; set; }
    public DbSet<GiftCard> GiftCards { get; set; }
    public DbSet<GiftCardTransaction> GiftCardTransactions { get; set; }
    public DbSet<DraftBill> DraftBills { get; set; }
    public DbSet<HeldBill> HeldBills { get; set; }
    public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }
    public DbSet<BillSequence> BillSequences { get; set; }
    public DbSet<PriceOverrideLog> PriceOverrideLogs { get; set; }
    public DbSet<SaleReturn> Returns { get; set; }
    public DbSet<ReturnItem> ReturnItems { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    // Cash Management & Organization
    public DbSet<CashTransaction> CashTransactions { get; set; }
    public DbSet<CompanyProfile> CompanyProfiles { get; set; }

    // Customer Credit Management
    public DbSet<CustomerTransaction> CustomerTransactions { get; set; }
    public DbSet<LoyaltySetting> LoyaltySettings { get; set; }

    // Quotations
    public DbSet<Quotation> Quotations { get; set; }
    public DbSet<QuotationItem> QuotationItems { get; set; }
    public DbSet<StockSummary> StockSummaries { get; set; }

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

        // Customers (BaseEntity + Name, Phone, Email, Address, LoyaltyPoints)
        var customer = modelBuilder.Entity<Customer>();
        customer.ToTable("Customers");
        customer.HasKey(c => c.Id);
        customer.Property(c => c.Id).HasColumnName("CustomerId");
        customer.Ignore(c => c.CustomerId);
        customer.Property(c => c.Name).IsRequired().HasMaxLength(200);
        customer.Property(c => c.Phone).HasMaxLength(20);
        customer.Property(c => c.Email).HasMaxLength(256);
        customer.Property(c => c.Address).HasMaxLength(500);
        customer.Property(c => c.LoyaltyPoints);
        customer.Property(c => c.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        customer.Property(c => c.UpdatedAt).HasColumnType("datetime");
        customer.Property(c => c.RowVersion)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        customer.Property(c => c.IsActive);
        customer.HasIndex(c => c.Phone).HasDatabaseName("IX_Customer_Phone");
        customer.HasIndex(c => c.Email).HasDatabaseName("IX_Customer_Email");
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

        // Suppliers
        var supplier = modelBuilder.Entity<Supplier>();
        supplier.ToTable("Suppliers");
        supplier.HasKey(s => s.Id);
        supplier.Property(s => s.Id).HasColumnName("SupplierId");
        supplier.Ignore(s => s.SupplierId);
        supplier.Property(s => s.Name).IsRequired().HasMaxLength(200);
        supplier.Property(s => s.Code).IsRequired().HasMaxLength(50);
        supplier.Property(s => s.ContactPerson).HasMaxLength(200);
        supplier.Property(s => s.Mobile).HasMaxLength(20);
        supplier.Property(s => s.Email).HasMaxLength(256);
        supplier.Property(s => s.Address).HasMaxLength(500);
        supplier.Property(s => s.GstVatNumber).HasMaxLength(15);
        supplier.Property(s => s.CreditPeriodDays);
        supplier.Property(s => s.CreditLimit).HasPrecision(18, 2);
        supplier.Property(s => s.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        supplier.Property(s => s.UpdatedAt).HasColumnType("datetime");
        supplier.Property(s => s.RowVersion)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        supplier.Property(s => s.IsActive);
        supplier.HasIndex(s => s.Code).IsUnique().HasDatabaseName("UX_Suppliers_Code");
        supplier.HasIndex(s => s.Name).HasDatabaseName("IX_Suppliers_Name");
        supplier.HasIndex(s => s.Mobile).HasDatabaseName("IX_Suppliers_Mobile");
        modelBuilder.Entity<Supplier>().HasQueryFilter(s => s.IsActive);

        // PurchaseOrders
        var purchaseOrder = modelBuilder.Entity<PurchaseOrder>();
        purchaseOrder.ToTable("PurchaseOrders");
        purchaseOrder.HasKey(po => po.Id);
        purchaseOrder.Property(po => po.Id).HasColumnName("PurchaseOrderId");
        purchaseOrder.Ignore(po => po.PurchaseOrderId);
        purchaseOrder.Property(po => po.SupplierId).IsRequired();
        purchaseOrder.Property(po => po.OrderDate).IsRequired().HasColumnType("datetime");
        purchaseOrder.Property(po => po.ExpectedDeliveryDate).HasColumnType("datetime");
        purchaseOrder.Property(po => po.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        purchaseOrder.Property(po => po.TotalAmount).HasPrecision(18, 2);
        purchaseOrder.Property(po => po.ReferenceNo).HasMaxLength(100);
        purchaseOrder.Property(po => po.Notes).HasMaxLength(500);
        purchaseOrder.Property(po => po.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        purchaseOrder.Property(po => po.UpdatedAt).HasColumnType("datetime");
        purchaseOrder.Property(po => po.RowVersion)
                     .HasColumnType("timestamp")
                     .ValueGeneratedOnAddOrUpdate()
                     .IsConcurrencyToken();
        purchaseOrder.Property(po => po.IsActive);
        purchaseOrder.HasOne(po => po.Supplier)
                     .WithMany()
                     .HasForeignKey(po => po.SupplierId)
                     .OnDelete(DeleteBehavior.Restrict);
        purchaseOrder.HasMany(po => po.Items)
                     .WithOne(poi => poi.PurchaseOrder)
                     .HasForeignKey(poi => poi.PurchaseOrderId)
                     .OnDelete(DeleteBehavior.Cascade);
        purchaseOrder.HasIndex(po => po.ReferenceNo).HasDatabaseName("IX_PurchaseOrders_ReferenceNo");
        purchaseOrder.HasIndex(po => po.SupplierId).HasDatabaseName("IX_PurchaseOrders_SupplierId");
        purchaseOrder.HasIndex(po => po.OrderDate).HasDatabaseName("IX_PurchaseOrders_OrderDate");
        modelBuilder.Entity<PurchaseOrder>().HasQueryFilter(po => po.IsActive);

        // PurchaseOrderItems
        var purchaseOrderItem = modelBuilder.Entity<PurchaseOrderItem>();
        purchaseOrderItem.ToTable("PurchaseOrderItems");
        purchaseOrderItem.HasKey(poi => poi.Id);
        purchaseOrderItem.Property(poi => poi.Id).HasColumnName("PurchaseOrderItemId");
        purchaseOrderItem.Ignore(poi => poi.PurchaseOrderItemId);
        purchaseOrderItem.Property(poi => poi.PurchaseOrderId).IsRequired();
        purchaseOrderItem.Property(poi => poi.ProductId).IsRequired();
        purchaseOrderItem.Property(poi => poi.Quantity).HasPrecision(12, 3);
        purchaseOrderItem.Property(poi => poi.UnitPrice).HasPrecision(18, 2);
        purchaseOrderItem.Property(poi => poi.TaxAmount).HasPrecision(18, 2);
        purchaseOrderItem.Property(poi => poi.TotalAmount).HasPrecision(18, 2);
        purchaseOrderItem.Property(poi => poi.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        purchaseOrderItem.Property(poi => poi.UpdatedAt).HasColumnType("datetime");
        purchaseOrderItem.Property(poi => poi.RowVersion)
                         .HasColumnType("timestamp")
                         .ValueGeneratedOnAddOrUpdate()
                         .IsConcurrencyToken();
        purchaseOrderItem.Property(poi => poi.IsActive);
        purchaseOrderItem.HasOne(poi => poi.Product)
                         .WithMany()
                         .HasForeignKey(poi => poi.ProductId)
                         .OnDelete(DeleteBehavior.Restrict);
        purchaseOrderItem.HasIndex(poi => poi.PurchaseOrderId).HasDatabaseName("IX_PurchaseOrderItems_PurchaseOrderId");
        purchaseOrderItem.HasIndex(poi => poi.ProductId).HasDatabaseName("IX_PurchaseOrderItems_ProductId");
        modelBuilder.Entity<PurchaseOrderItem>().HasQueryFilter(poi => poi.IsActive);

        // PurchaseEntries
        var purchaseEntry = modelBuilder.Entity<PurchaseEntry>();
        purchaseEntry.ToTable("PurchaseEntries");
        purchaseEntry.HasKey(pe => pe.Id);
        purchaseEntry.Property(pe => pe.Id).HasColumnName("PurchaseEntryId");
        purchaseEntry.Ignore(pe => pe.PurchaseEntryId);
        purchaseEntry.Property(pe => pe.SupplierId).IsRequired();
        purchaseEntry.Property(pe => pe.PurchaseOrderId);
        purchaseEntry.Property(pe => pe.InvoiceNo).IsRequired().HasMaxLength(100);
        purchaseEntry.Property(pe => pe.InvoiceDate).IsRequired().HasColumnType("datetime");
        purchaseEntry.Property(pe => pe.ReceivedDate).IsRequired().HasColumnType("datetime");
        purchaseEntry.Property(pe => pe.TotalAmount).HasPrecision(18, 2);
        purchaseEntry.Property(pe => pe.TaxAmount).HasPrecision(18, 2);
        purchaseEntry.Property(pe => pe.Notes).HasMaxLength(500);
        purchaseEntry.Property(pe => pe.IsProcessed);
        purchaseEntry.Property(pe => pe.ProcessedAt).HasColumnType("datetime");
        purchaseEntry.Property(pe => pe.ProcessedBy).HasMaxLength(100);
        purchaseEntry.Property(pe => pe.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        purchaseEntry.Property(pe => pe.UpdatedAt).HasColumnType("datetime");
        purchaseEntry.Property(pe => pe.RowVersion)
                     .HasColumnType("timestamp")
                     .ValueGeneratedOnAddOrUpdate()
                     .IsConcurrencyToken();
        purchaseEntry.Property(pe => pe.IsActive);
        purchaseEntry.HasOne(pe => pe.Supplier)
                     .WithMany()
                     .HasForeignKey(pe => pe.SupplierId)
                     .OnDelete(DeleteBehavior.Restrict);
        purchaseEntry.HasOne(pe => pe.PurchaseOrder)
                     .WithMany()
                     .HasForeignKey(pe => pe.PurchaseOrderId)
                     .OnDelete(DeleteBehavior.SetNull);
        purchaseEntry.HasMany(pe => pe.Items)
                     .WithOne(pei => pei.PurchaseEntry)
                     .HasForeignKey(pei => pei.PurchaseEntryId)
                     .OnDelete(DeleteBehavior.Cascade);
        purchaseEntry.HasIndex(pe => pe.InvoiceNo).HasDatabaseName("IX_PurchaseEntries_InvoiceNo");
        purchaseEntry.HasIndex(pe => pe.SupplierId).HasDatabaseName("IX_PurchaseEntries_SupplierId");
        purchaseEntry.HasIndex(pe => pe.PurchaseOrderId).HasDatabaseName("IX_PurchaseEntries_PurchaseOrderId");
        purchaseEntry.HasIndex(pe => pe.ReceivedDate).HasDatabaseName("IX_PurchaseEntries_ReceivedDate");
        modelBuilder.Entity<PurchaseEntry>().HasQueryFilter(pe => pe.IsActive);

        // PurchaseEntryItems
        var purchaseEntryItem = modelBuilder.Entity<PurchaseEntryItem>();
        purchaseEntryItem.ToTable("PurchaseEntryItems");
        purchaseEntryItem.HasKey(pei => pei.Id);
        purchaseEntryItem.Property(pei => pei.Id).HasColumnName("PurchaseEntryItemId");
        purchaseEntryItem.Ignore(pei => pei.PurchaseEntryItemId);
        purchaseEntryItem.Property(pei => pei.PurchaseEntryId).IsRequired();
        purchaseEntryItem.Property(pei => pei.ProductId).IsRequired();
        purchaseEntryItem.Property(pei => pei.BatchNo).HasMaxLength(100);
        purchaseEntryItem.Property(pei => pei.ExpiryDate).HasColumnType("date");
        purchaseEntryItem.Property(pei => pei.Quantity).HasPrecision(12, 3);
        purchaseEntryItem.Property(pei => pei.CostPrice).HasPrecision(18, 2);
        purchaseEntryItem.Property(pei => pei.SellingPrice).HasPrecision(18, 2);
        purchaseEntryItem.Property(pei => pei.MRP).HasPrecision(18, 2);
        purchaseEntryItem.Property(pei => pei.TaxAmount).HasPrecision(18, 2);
        purchaseEntryItem.Property(pei => pei.TotalAmount).HasPrecision(18, 2);
        purchaseEntryItem.Property(pei => pei.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        purchaseEntryItem.Property(pei => pei.UpdatedAt).HasColumnType("datetime");
        purchaseEntryItem.Property(pei => pei.RowVersion)
                         .HasColumnType("timestamp")
                         .ValueGeneratedOnAddOrUpdate()
                         .IsConcurrencyToken();
        purchaseEntryItem.Property(pei => pei.IsActive);
        purchaseEntryItem.HasOne(pei => pei.Product)
                         .WithMany()
                         .HasForeignKey(pei => pei.ProductId)
                         .OnDelete(DeleteBehavior.Restrict);
        purchaseEntryItem.HasIndex(pei => pei.PurchaseEntryId).HasDatabaseName("IX_PurchaseEntryItems_PurchaseEntryId");
        purchaseEntryItem.HasIndex(pei => pei.ProductId).HasDatabaseName("IX_PurchaseEntryItems_ProductId");
        purchaseEntryItem.HasIndex(pei => pei.BatchNo).HasDatabaseName("IX_PurchaseEntryItems_BatchNo");
        modelBuilder.Entity<PurchaseEntryItem>().HasQueryFilter(pei => pei.IsActive);

        // Batches (Stock Management)
        var batch = modelBuilder.Entity<Batch>();
        batch.ToTable("Batches");
        batch.HasKey(b => b.Id);
        batch.Property(b => b.Id).HasColumnName("BatchId");
        batch.Ignore(b => b.BatchId);
        batch.Property(b => b.ProductId).IsRequired();
        batch.Property(b => b.BatchNo).IsRequired().HasMaxLength(100);
        batch.Property(b => b.ExpiryDate).HasColumnType("date");
        batch.Property(b => b.ManufactureDate).HasColumnType("date");
        batch.Property(b => b.CostPrice).HasPrecision(18, 2);
        batch.Property(b => b.SellingPrice).HasPrecision(18, 2);
        batch.Property(b => b.MRP).HasPrecision(18, 2);
        batch.Property(b => b.ReceivedQuantity).HasPrecision(12, 3);
        batch.Property(b => b.CurrentQuantity).HasPrecision(12, 3);
        batch.Property(b => b.AllocatedQuantity).HasPrecision(12, 3);
        batch.Property(b => b.SoldQuantity).HasPrecision(12, 3);
        batch.Property(b => b.ReturnedQuantity).HasPrecision(12, 3);
        batch.Property(b => b.AdjustedQuantity).HasPrecision(12, 3);
        batch.Property(b => b.PurchaseEntryId);
        batch.Property(b => b.PurchaseEntryItemId);
        batch.Property(b => b.SupplierId).IsRequired();
        batch.Property(b => b.LocationCode).HasMaxLength(50);
        batch.Property(b => b.BinLocation).HasMaxLength(50);
        batch.Property(b => b.ReorderLevel).HasPrecision(12, 3);
        batch.Property(b => b.ReceivedDate).HasColumnType("datetime");
        batch.Property(b => b.ReceivedBy).HasMaxLength(100);
        batch.Property(b => b.LastTransactionDate).HasColumnType("datetime");
        batch.Property(b => b.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        batch.Property(b => b.UpdatedAt).HasColumnType("datetime");
        batch.Property(b => b.RowVersion)
              .HasColumnType("timestamp")
              .ValueGeneratedOnAddOrUpdate()
              .IsConcurrencyToken();
        batch.Property(b => b.IsActive);
        batch.Ignore(b => b.IsExpired);
        batch.Ignore(b => b.IsLowStock);
        batch.Ignore(b => b.AvailableQuantity);
        batch.HasOne(b => b.Product)
             .WithMany()
             .HasForeignKey(b => b.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        batch.HasOne(b => b.Supplier)
             .WithMany()
             .HasForeignKey(b => b.SupplierId)
             .OnDelete(DeleteBehavior.Restrict);
        batch.HasOne(b => b.PurchaseEntry)
             .WithMany()
             .HasForeignKey(b => b.PurchaseEntryId)
             .OnDelete(DeleteBehavior.SetNull);
        batch.HasOne(b => b.PurchaseEntryItem)
             .WithMany()
             .HasForeignKey(b => b.PurchaseEntryItemId)
             .OnDelete(DeleteBehavior.SetNull);
        batch.HasIndex(b => b.ProductId).HasDatabaseName("IX_Batches_ProductId");
        batch.HasIndex(b => b.BatchNo).HasDatabaseName("IX_Batches_BatchNo");
        batch.HasIndex(b => b.ExpiryDate).HasDatabaseName("IX_Batches_ExpiryDate");
        batch.HasIndex(b => b.SupplierId).HasDatabaseName("IX_Batches_SupplierId");
        batch.HasIndex(b => b.PurchaseEntryId).HasDatabaseName("IX_Batches_PurchaseEntryId");
        batch.HasIndex(b => new { b.ProductId, b.BatchNo }).HasDatabaseName("IX_Batches_Product_Batch");
        modelBuilder.Entity<Batch>().HasQueryFilter(b => b.IsActive);

        // PurchaseReturns
        var purchaseReturn = modelBuilder.Entity<PurchaseReturn>();
        purchaseReturn.ToTable("PurchaseReturns");
        purchaseReturn.HasKey(pr => pr.Id);
        purchaseReturn.Property(pr => pr.Id).HasColumnName("PurchaseReturnId");
        purchaseReturn.Ignore(pr => pr.PurchaseReturnId);
        purchaseReturn.Property(pr => pr.SupplierId).IsRequired();
        purchaseReturn.Property(pr => pr.PurchaseEntryId);
        purchaseReturn.Property(pr => pr.ReturnNo).IsRequired().HasMaxLength(100);
        purchaseReturn.Property(pr => pr.ReturnDate).IsRequired().HasColumnType("datetime");
        purchaseReturn.Property(pr => pr.TotalAmount).HasPrecision(18, 2);
        purchaseReturn.Property(pr => pr.TaxAmount).HasPrecision(18, 2);
        purchaseReturn.Property(pr => pr.Reason).HasMaxLength(500);
        purchaseReturn.Property(pr => pr.Notes).HasMaxLength(500);
        purchaseReturn.Property(pr => pr.Status).IsRequired().HasMaxLength(20);
        purchaseReturn.Property(pr => pr.IsProcessed);
        purchaseReturn.Property(pr => pr.ProcessedAt).HasColumnType("datetime");
        purchaseReturn.Property(pr => pr.ProcessedBy).HasMaxLength(100);
        purchaseReturn.Property(pr => pr.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        purchaseReturn.Property(pr => pr.UpdatedAt).HasColumnType("datetime");
        purchaseReturn.Property(pr => pr.RowVersion)
                     .HasColumnType("timestamp")
                     .ValueGeneratedOnAddOrUpdate()
                     .IsConcurrencyToken();
        purchaseReturn.HasOne(pr => pr.Supplier)
                     .WithMany()
                     .HasForeignKey(pr => pr.SupplierId)
                     .OnDelete(DeleteBehavior.Restrict);
        purchaseReturn.HasOne(pr => pr.PurchaseEntry)
                     .WithMany()
                     .HasForeignKey(pr => pr.PurchaseEntryId)
                     .OnDelete(DeleteBehavior.SetNull);
        purchaseReturn.HasMany(pr => pr.Items)
                     .WithOne(pri => pri.PurchaseReturn)
                     .HasForeignKey(pri => pri.PurchaseReturnId)
                     .OnDelete(DeleteBehavior.Cascade);
        purchaseReturn.HasIndex(pr => pr.ReturnNo).HasDatabaseName("IX_PurchaseReturns_ReturnNo");
        purchaseReturn.HasIndex(pr => pr.SupplierId).HasDatabaseName("IX_PurchaseReturns_SupplierId");
        purchaseReturn.HasIndex(pr => pr.PurchaseEntryId).HasDatabaseName("IX_PurchaseReturns_PurchaseEntryId");
        purchaseReturn.HasIndex(pr => pr.ReturnDate).HasDatabaseName("IX_PurchaseReturns_ReturnDate");
        modelBuilder.Entity<PurchaseReturn>().HasQueryFilter(pr => pr.IsActive);

        // PurchaseReturnItems
        var purchaseReturnItem = modelBuilder.Entity<PurchaseReturnItem>();
        purchaseReturnItem.ToTable("PurchaseReturnItems");
        purchaseReturnItem.HasKey(pri => pri.Id);
        purchaseReturnItem.Property(pri => pri.Id).HasColumnName("PurchaseReturnItemId");
        purchaseReturnItem.Ignore(pri => pri.PurchaseReturnItemId);
        purchaseReturnItem.Property(pri => pri.PurchaseReturnId).IsRequired();
        purchaseReturnItem.Property(pri => pri.ProductId).IsRequired();
        purchaseReturnItem.Property(pri => pri.PurchaseEntryItemId);
        purchaseReturnItem.Property(pri => pri.BatchNo).HasMaxLength(100);
        purchaseReturnItem.Property(pri => pri.ExpiryDate).HasColumnType("date");
        purchaseReturnItem.Property(pri => pri.Quantity).HasPrecision(12, 3);
        purchaseReturnItem.Property(pri => pri.UnitPrice).HasPrecision(18, 2);
        purchaseReturnItem.Property(pri => pri.TaxAmount).HasPrecision(18, 2);
        purchaseReturnItem.Property(pri => pri.TotalAmount).HasPrecision(18, 2);
        purchaseReturnItem.Property(pri => pri.Reason).HasMaxLength(500);
        purchaseReturnItem.Property(pri => pri.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        purchaseReturnItem.Property(pri => pri.UpdatedAt).HasColumnType("datetime");
        purchaseReturnItem.Property(pri => pri.RowVersion)
                         .HasColumnType("timestamp")
                         .ValueGeneratedOnAddOrUpdate()
                         .IsConcurrencyToken();
        purchaseReturnItem.HasOne(pri => pri.Product)
                         .WithMany()
                         .HasForeignKey(pri => pri.ProductId)
                         .OnDelete(DeleteBehavior.Restrict);
        purchaseReturnItem.HasIndex(pri => pri.PurchaseReturnId).HasDatabaseName("IX_PurchaseReturnItems_PurchaseReturnId");
        purchaseReturnItem.HasIndex(pri => pri.ProductId).HasDatabaseName("IX_PurchaseReturnItems_ProductId");
        purchaseReturnItem.HasIndex(pri => pri.BatchNo).HasDatabaseName("IX_PurchaseReturnItems_BatchNo");
        modelBuilder.Entity<PurchaseReturnItem>().HasQueryFilter(pri => pri.IsActive);

        // StockLedgerEntries
        var stockLedgerEntry = modelBuilder.Entity<StockLedgerEntry>();
        stockLedgerEntry.ToTable("StockLedgerEntries");
        stockLedgerEntry.HasKey(sle => sle.StockEntryId);
        stockLedgerEntry.Property(sle => sle.StockEntryId).ValueGeneratedOnAdd();
        stockLedgerEntry.Property(sle => sle.ProductId).IsRequired().HasColumnType("BIGINT");
        stockLedgerEntry.Property(sle => sle.Quantity).HasPrecision(12, 3);
        stockLedgerEntry.Property(sle => sle.EntryType).IsRequired().HasMaxLength(20);
        stockLedgerEntry.Property(sle => sle.ReferenceType).IsRequired().HasMaxLength(50);
        stockLedgerEntry.Property(sle => sle.ReferenceId);
        stockLedgerEntry.Property(sle => sle.EntryDate).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        stockLedgerEntry.Property(sle => sle.Remarks).HasMaxLength(500);
        stockLedgerEntry.HasIndex(sle => sle.ProductId).HasDatabaseName("IX_StockLedgerEntries_ProductId");
        stockLedgerEntry.HasIndex(sle => sle.ReferenceId).HasDatabaseName("IX_StockLedgerEntries_ReferenceId");
        stockLedgerEntry.HasIndex(sle => sle.EntryDate).HasDatabaseName("IX_StockLedgerEntries_EntryDate");

        // ----- Billing module -----

        var sale = modelBuilder.Entity<Sale>();
        sale.ToTable("Sales");
        sale.HasKey(s => s.SaleId);
        sale.Property(s => s.SaleId).ValueGeneratedOnAdd();
        sale.Property(s => s.BillNumber).IsRequired().HasMaxLength(50);
        sale.Property(s => s.InvoiceNumber).HasMaxLength(50);
        sale.Property(s => s.SaleType).HasConversion<string>().HasMaxLength(20);
        sale.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        sale.Property(s => s.CustomerName).HasMaxLength(200);
        sale.Property(s => s.CustomerPhone).HasMaxLength(20);
        sale.Property(s => s.CustomerGSTIN).HasMaxLength(15);
        sale.Property(s => s.Subtotal).HasPrecision(18, 2);
        sale.Property(s => s.DiscountPercent).HasPrecision(5, 2);
        sale.Property(s => s.DiscountAmount).HasPrecision(18, 2);
        sale.Property(s => s.TaxableAmount).HasPrecision(18, 2);
        sale.Property(s => s.CGST).HasPrecision(18, 2);
        sale.Property(s => s.SGST).HasPrecision(18, 2);
        sale.Property(s => s.IGST).HasPrecision(18, 2);
        sale.Property(s => s.Cess).HasPrecision(18, 2);
        sale.Property(s => s.TotalTax).HasPrecision(18, 2);
        sale.Property(s => s.RoundOff).HasPrecision(18, 2);
        sale.Property(s => s.TotalAmount).HasPrecision(18, 2);
        sale.Property(s => s.RedemptionAmount).HasPrecision(18, 2);
        sale.Property(s => s.CouponCode).HasMaxLength(50);
        sale.Property(s => s.PaymentStatus).HasConversion<string>().HasMaxLength(20);
        sale.Property(s => s.DraftName).HasMaxLength(100);
        sale.Property(s => s.Notes).HasMaxLength(500);
        sale.Property(s => s.TerminalId).HasMaxLength(50);
        sale.Property(s => s.CreatedBy).IsRequired().HasMaxLength(50);
        sale.Property(s => s.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        sale.Property(s => s.UpdatedBy).HasMaxLength(50);
        sale.Property(s => s.UpdatedAt).HasColumnType("datetime").ValueGeneratedOnUpdate();
        sale.HasOne(s => s.Customer).WithMany().HasForeignKey(s => s.CustomerId).OnDelete(DeleteBehavior.SetNull);
        sale.HasIndex(s => s.BillNumber).IsUnique().HasDatabaseName("uk_sales_billnumber");
        sale.HasIndex(s => s.InvoiceNumber).IsUnique().HasDatabaseName("uk_sales_invoicenumber");
        sale.HasIndex(s => s.CustomerId).HasDatabaseName("idx_sales_customerid");
        sale.HasIndex(s => s.Status).HasDatabaseName("idx_sales_status");
        sale.HasIndex(s => s.CreatedAt).HasDatabaseName("idx_sales_createdat");
        sale.Property(s => s.IsLocked);
        // Omit LockedAt/LockedBy from INSERT/UPDATE until DB has these columns (run migration or SQL below).
        sale.Ignore(s => s.LockedAt);
        sale.Ignore(s => s.LockedBy);
        sale.HasIndex(s => new { s.IsDraft, s.IsHeld }).HasDatabaseName("idx_sales_draft_held");

        var saleItem = modelBuilder.Entity<SaleItem>();
        saleItem.ToTable("SaleItems");
        saleItem.HasKey(si => si.SaleItemId);
        saleItem.Property(si => si.SaleItemId).ValueGeneratedOnAdd();
        saleItem.Property(si => si.ProductName).IsRequired().HasMaxLength(200);
        saleItem.Property(si => si.SKU).IsRequired().HasMaxLength(100);
        saleItem.Property(si => si.Barcode).HasMaxLength(100);
        saleItem.Property(si => si.HSNCode).HasMaxLength(20);
        saleItem.Property(si => si.Quantity).HasPrecision(18, 3);
        saleItem.Property(si => si.UnitName).IsRequired().HasMaxLength(50);
        saleItem.Property(si => si.MRP).HasPrecision(18, 2);
        saleItem.Property(si => si.SellingPrice).HasPrecision(18, 2);
        saleItem.Property(si => si.ActualPrice).HasPrecision(18, 2);
        saleItem.Property(si => si.DiscountPercent).HasPrecision(5, 2);
        saleItem.Property(si => si.DiscountAmount).HasPrecision(18, 2);
        saleItem.Property(si => si.TaxRate).HasPrecision(5, 2);
        saleItem.Property(si => si.CGST).HasPrecision(18, 2);
        saleItem.Property(si => si.SGST).HasPrecision(18, 2);
        saleItem.Property(si => si.IGST).HasPrecision(18, 2);
        saleItem.Property(si => si.Cess).HasPrecision(18, 2);
        saleItem.Property(si => si.TaxAmount).HasPrecision(18, 2);
        saleItem.Property(si => si.Subtotal).HasPrecision(18, 2);
        saleItem.Property(si => si.TotalAmount).HasPrecision(18, 2);
        saleItem.Property(si => si.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        saleItem.HasOne(si => si.Sale).WithMany(s => s.SaleItems).HasForeignKey(si => si.SaleId).OnDelete(DeleteBehavior.Cascade);
        saleItem.HasOne<Product>().WithMany().HasForeignKey(si => si.ProductId).OnDelete(DeleteBehavior.Restrict);
        saleItem.HasOne<Uom>().WithMany().HasForeignKey(si => si.UomId).OnDelete(DeleteBehavior.Restrict);
        saleItem.HasOne<TaxProfile>().WithMany().HasForeignKey(si => si.TaxProfileId).OnDelete(DeleteBehavior.Restrict);
        saleItem.HasIndex(si => si.SaleId).HasDatabaseName("idx_saleitems_saleid");
        saleItem.HasIndex(si => si.ProductId).HasDatabaseName("idx_saleitems_productid");

        var payment = modelBuilder.Entity<Payment>();
        payment.ToTable("Payments");
        payment.HasKey(p => p.PaymentId);
        payment.Property(p => p.PaymentId).ValueGeneratedOnAdd();
        payment.Property(p => p.PaymentMethod).HasConversion<string>().HasMaxLength(30);
        payment.Property(p => p.Amount).HasPrecision(18, 2);
        payment.Property(p => p.ReferenceNumber).HasMaxLength(100);
        payment.Property(p => p.CardType).HasConversion<string>().HasMaxLength(10);
        payment.Property(p => p.CardLastFour).HasMaxLength(4);
        payment.Property(p => p.UPIId).HasMaxLength(100);
        payment.Property(p => p.BankName).HasMaxLength(100);
        payment.Property(p => p.TenderedAmount).HasPrecision(18, 2);
        payment.Property(p => p.ChangeAmount).HasPrecision(18, 2);
        payment.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        payment.Property(p => p.GatewayTransactionId).HasMaxLength(100);
        payment.Property(p => p.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        payment.HasOne(p => p.Sale).WithMany(s => s.Payments).HasForeignKey(p => p.SaleId).OnDelete(DeleteBehavior.Cascade);
        payment.HasIndex(p => p.SaleId).HasDatabaseName("idx_payments_saleid");
        payment.HasIndex(p => p.ReferenceNumber).HasDatabaseName("idx_payments_reference");

        var voucher = modelBuilder.Entity<Voucher>();
        voucher.ToTable("Vouchers");
        voucher.HasKey(v => v.VoucherId);
        voucher.Property(v => v.Code).IsRequired().HasMaxLength(50);
        voucher.Property(v => v.Name).IsRequired().HasMaxLength(200);
        voucher.Property(v => v.DiscountType).HasConversion<string>().HasMaxLength(20);
        voucher.Property(v => v.DiscountValue).HasPrecision(18, 2);
        voucher.Property(v => v.MaxDiscountAmount).HasPrecision(18, 2);
        voucher.Property(v => v.MinPurchaseAmount).HasPrecision(18, 2);
        voucher.Property(v => v.ApplicableCategories).HasColumnType("json");
        voucher.Property(v => v.ApplicableBrands).HasColumnType("json");
        voucher.Property(v => v.ApplicableProducts).HasColumnType("json");
        voucher.Property(v => v.CreatedBy).IsRequired().HasMaxLength(50);
        voucher.Property(v => v.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        voucher.Property(v => v.UpdatedBy).HasMaxLength(50);
        voucher.Property(v => v.UpdatedAt).HasColumnType("datetime").ValueGeneratedOnUpdate();
        voucher.HasIndex(v => v.Code).IsUnique().HasDatabaseName("uk_vouchers_code");
        voucher.HasIndex(v => new { v.ValidFrom, v.ValidTo }).HasDatabaseName("idx_vouchers_validity");

        var giftCard = modelBuilder.Entity<GiftCard>();
        giftCard.ToTable("GiftCards");
        giftCard.HasKey(g => g.GiftCardId);
        giftCard.Property(g => g.CardNumber).IsRequired().HasMaxLength(50);
        giftCard.Property(g => g.PIN).HasMaxLength(10);
        giftCard.Property(g => g.InitialBalance).HasPrecision(18, 2);
        giftCard.Property(g => g.CurrentBalance).HasPrecision(18, 2);
        giftCard.Property(g => g.ValidFrom).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        giftCard.Property(g => g.CreatedBy).IsRequired().HasMaxLength(50);
        giftCard.Property(g => g.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        giftCard.Property(g => g.UpdatedAt).HasColumnType("datetime").ValueGeneratedOnUpdate();
        giftCard.HasOne(g => g.Customer).WithMany().HasForeignKey(g => g.CustomerId).OnDelete(DeleteBehavior.SetNull);
        giftCard.HasIndex(g => g.CardNumber).IsUnique().HasDatabaseName("uk_giftcards_cardnumber");

        var giftCardTxn = modelBuilder.Entity<GiftCardTransaction>();
        giftCardTxn.ToTable("GiftCardTransactions");
        giftCardTxn.HasKey(g => g.TransactionId);
        giftCardTxn.Property(g => g.TransactionType).HasConversion<string>().HasMaxLength(20);
        giftCardTxn.Property(g => g.Amount).HasPrecision(18, 2);
        giftCardTxn.Property(g => g.BalanceAfter).HasPrecision(18, 2);
        giftCardTxn.Property(g => g.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        giftCardTxn.HasOne(g => g.GiftCard).WithMany().HasForeignKey(g => g.GiftCardId).OnDelete(DeleteBehavior.Restrict);
        giftCardTxn.HasOne<Sale>().WithMany().HasForeignKey(g => g.SaleId).OnDelete(DeleteBehavior.SetNull);
        giftCardTxn.HasIndex(g => g.GiftCardId).HasDatabaseName("idx_giftcardtxn_giftcardid");

        var draftBill = modelBuilder.Entity<DraftBill>();
        draftBill.ToTable("DraftBills");
        draftBill.HasKey(d => d.DraftBillId);
        draftBill.Property(d => d.DraftName).IsRequired().HasMaxLength(100);
        draftBill.Property(d => d.CartData).IsRequired().HasColumnType("json");
        draftBill.Property(d => d.CreatedBy).IsRequired().HasMaxLength(50);
        draftBill.Property(d => d.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        draftBill.Property(d => d.UpdatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnUpdate();
        draftBill.HasIndex(d => d.CreatedBy).HasDatabaseName("idx_draftbills_createdby");

        var heldBill = modelBuilder.Entity<HeldBill>();
        heldBill.ToTable("HeldBills");
        heldBill.HasKey(h => h.HeldBillId);
        heldBill.Property(h => h.HoldReference).IsRequired().HasMaxLength(100);
        heldBill.Property(h => h.CustomerName).HasMaxLength(200);
        heldBill.Property(h => h.CartData).IsRequired().HasColumnType("json");
        heldBill.Property(h => h.TotalAmount).HasPrecision(18, 2);
        heldBill.Property(h => h.HeldBy).IsRequired().HasMaxLength(50);
        heldBill.Property(h => h.HeldAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        heldBill.Property(h => h.RetrievedBy).HasMaxLength(50);
        heldBill.HasIndex(h => h.HeldBy).HasDatabaseName("idx_heldbills_heldby");
        heldBill.HasIndex(h => h.ExpiresAt).HasDatabaseName("idx_heldbills_expiresat");
        heldBill.HasIndex(h => h.IsRetrieved).HasDatabaseName("idx_heldbills_isretrieved");

        var loyaltyTxn = modelBuilder.Entity<LoyaltyTransaction>();
        loyaltyTxn.ToTable("LoyaltyTransactions");
        loyaltyTxn.HasKey(l => l.TransactionId);
        loyaltyTxn.Property(l => l.TransactionType).HasConversion<string>().HasMaxLength(20);
        loyaltyTxn.Property(l => l.Description).HasMaxLength(200);
        loyaltyTxn.Property(l => l.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        loyaltyTxn.HasOne(l => l.Customer).WithMany().HasForeignKey(l => l.CustomerId).OnDelete(DeleteBehavior.Cascade);
        loyaltyTxn.HasOne<Sale>().WithMany().HasForeignKey(l => l.SaleId).OnDelete(DeleteBehavior.SetNull);
        loyaltyTxn.HasIndex(l => l.CustomerId).HasDatabaseName("idx_loyaltytxn_customerid");

        var billSeq = modelBuilder.Entity<BillSequence>();
        billSeq.ToTable("BillSequence");
        billSeq.HasKey(b => b.SequenceId);
        billSeq.Property(b => b.Prefix).IsRequired().HasMaxLength(10);
        billSeq.HasIndex(b => new { b.Year, b.Month }).IsUnique().HasDatabaseName("uk_billsequence_year_month");

        var priceLog = modelBuilder.Entity<PriceOverrideLog>();
        priceLog.ToTable("PriceOverrideLog");
        priceLog.HasKey(p => p.LogId);
        priceLog.Property(p => p.OriginalPrice).HasPrecision(18, 2);
        priceLog.Property(p => p.OverriddenPrice).HasPrecision(18, 2);
        priceLog.Property(p => p.Reason).HasMaxLength(200);
        priceLog.Property(p => p.OverriddenBy).IsRequired().HasMaxLength(50);
        priceLog.Property(p => p.OverriddenAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        priceLog.HasOne<Sale>().WithMany().HasForeignKey(p => p.SaleId).OnDelete(DeleteBehavior.Cascade);
        priceLog.HasIndex(p => p.SaleId).HasDatabaseName("idx_pricelog_saleid");

        var saleReturn = modelBuilder.Entity<SaleReturn>();
        saleReturn.ToTable("Returns");
        saleReturn.HasKey(r => r.ReturnId);
        saleReturn.Property(r => r.ReturnNumber).IsRequired().HasMaxLength(50);
        saleReturn.Property(r => r.ReturnType).HasConversion<string>().HasMaxLength(20);
        saleReturn.Property(r => r.TotalReturnAmount).HasPrecision(18, 2);
        saleReturn.Property(r => r.RefundAmount).HasPrecision(18, 2);
        saleReturn.Property(r => r.Reason).HasMaxLength(500);
        saleReturn.Property(r => r.CreatedBy).IsRequired().HasMaxLength(50);
        saleReturn.Property(r => r.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        saleReturn.Property(r => r.ReturnDate).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        saleReturn.Property(r => r.RefundMode).HasMaxLength(20).HasDefaultValue("Cash");
        saleReturn.Property(r => r.Status).HasMaxLength(20).HasDefaultValue("Draft");
        saleReturn.Property(r => r.IsProcessed).HasDefaultValue(false);
        saleReturn.HasOne(r => r.OriginalSale).WithMany().HasForeignKey(r => r.OriginalSaleId).OnDelete(DeleteBehavior.Restrict);
        saleReturn.HasOne(r => r.NewSale).WithMany().HasForeignKey(r => r.NewSaleId).OnDelete(DeleteBehavior.SetNull);
        saleReturn.HasOne(r => r.Customer).WithMany().HasForeignKey(r => r.CustomerId).OnDelete(DeleteBehavior.SetNull);
        saleReturn.HasIndex(r => r.ReturnNumber).IsUnique().HasDatabaseName("uk_returns_returnnumber");
        saleReturn.HasIndex(r => r.OriginalSaleId).HasDatabaseName("idx_returns_originalsaleid");
        saleReturn.HasIndex(r => r.ReturnDate).HasDatabaseName("idx_returns_returndate");

        var returnItem = modelBuilder.Entity<ReturnItem>();
        returnItem.ToTable("ReturnItems");
        returnItem.HasKey(ri => ri.ReturnItemId);
        returnItem.Property(ri => ri.QuantityReturned).HasPrecision(18, 3);
        returnItem.Property(ri => ri.ReturnAmount).HasPrecision(18, 2);
        returnItem.Property(ri => ri.RefundPrice).HasPrecision(18, 2);
        returnItem.Property(ri => ri.ProductName).HasMaxLength(200);
        returnItem.Property(ri => ri.IsRestockable).HasDefaultValue(true);
        returnItem.Property(ri => ri.Reason).HasMaxLength(500);
        returnItem.HasOne(ri => ri.SaleReturn).WithMany(r => r.ReturnItems).HasForeignKey(ri => ri.ReturnId).OnDelete(DeleteBehavior.Cascade);
        returnItem.HasOne(ri => ri.SaleItem).WithMany().HasForeignKey(ri => ri.SaleItemId).OnDelete(DeleteBehavior.Restrict);
        returnItem.HasIndex(ri => ri.ReturnId).HasDatabaseName("idx_returnitems_returnid");

        var auditLog = modelBuilder.Entity<AuditLog>();
        auditLog.ToTable("AuditLogs");
        auditLog.HasKey(a => a.AuditLogId);
        auditLog.Property(a => a.AuditLogId).ValueGeneratedOnAdd();
        auditLog.Property(a => a.UserId).IsRequired().HasMaxLength(100);
        auditLog.Property(a => a.Action).IsRequired().HasMaxLength(100);
        auditLog.Property(a => a.EntityType).IsRequired().HasMaxLength(50);
        auditLog.Property(a => a.EntityId).HasMaxLength(100);
        auditLog.Property(a => a.OldValue).HasColumnType("text");
        auditLog.Property(a => a.NewValue).HasColumnType("text");
        auditLog.Property(a => a.IPAddress).HasMaxLength(50);
        auditLog.Property(a => a.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        auditLog.HasIndex(a => a.CreatedAt).HasDatabaseName("idx_auditlogs_createdat");
        auditLog.HasIndex(a => a.UserId).HasDatabaseName("idx_auditlogs_userid");
        auditLog.HasIndex(a => a.Action).HasDatabaseName("idx_auditlogs_action");

        // SupplierPayments
        var supplierPayment = modelBuilder.Entity<SupplierPayment>();
        supplierPayment.ToTable("SupplierPayments");
        supplierPayment.HasKey(sp => sp.Id);
        supplierPayment.Property(sp => sp.Id).HasColumnName("SupplierPaymentId");
        supplierPayment.Ignore(sp => sp.SupplierPaymentId);
        supplierPayment.Property(sp => sp.SupplierId).IsRequired();
        supplierPayment.Property(sp => sp.PaymentDate).IsRequired().HasColumnType("datetime");
        supplierPayment.Property(sp => sp.Amount).HasPrecision(18, 2);
        supplierPayment.Property(sp => sp.PaymentMode).IsRequired().HasMaxLength(20);
        supplierPayment.Property(sp => sp.ReferenceNo).HasMaxLength(100);
        supplierPayment.Property(sp => sp.BankName).HasMaxLength(100);
        supplierPayment.Property(sp => sp.Remarks).HasMaxLength(500);
        supplierPayment.Property(sp => sp.PaymentNo).IsRequired().HasMaxLength(50);
        supplierPayment.Property(sp => sp.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        supplierPayment.Property(sp => sp.UpdatedAt).HasColumnType("datetime");
        supplierPayment.Property(sp => sp.RowVersion)
                       .HasColumnType("timestamp")
                       .ValueGeneratedOnAddOrUpdate()
                       .IsConcurrencyToken();
        supplierPayment.Property(sp => sp.IsActive);
        supplierPayment.HasOne(sp => sp.Supplier)
                       .WithMany()
                       .HasForeignKey(sp => sp.SupplierId)
                       .OnDelete(DeleteBehavior.Restrict);
        supplierPayment.HasIndex(sp => sp.PaymentNo).IsUnique().HasDatabaseName("UX_SupplierPayments_PaymentNo");
        supplierPayment.HasIndex(sp => sp.SupplierId).HasDatabaseName("IX_SupplierPayments_SupplierId");
        supplierPayment.HasIndex(sp => sp.PaymentDate).HasDatabaseName("IX_SupplierPayments_PaymentDate");
        modelBuilder.Entity<SupplierPayment>().HasQueryFilter(sp => sp.IsActive);

        // SupplierTransactions (Ledger)
        var supplierTransaction = modelBuilder.Entity<SupplierTransaction>();
        supplierTransaction.ToTable("SupplierTransactions");
        supplierTransaction.HasKey(st => st.Id);
        supplierTransaction.Property(st => st.Id).HasColumnName("SupplierTransactionId");
        supplierTransaction.Ignore(st => st.SupplierTransactionId);
        supplierTransaction.Property(st => st.SupplierId).IsRequired();
        supplierTransaction.Property(st => st.TransactionDate).IsRequired().HasColumnType("datetime");
        supplierTransaction.Property(st => st.TransactionType).IsRequired().HasMaxLength(20);
        supplierTransaction.Property(st => st.ReferenceId);
        supplierTransaction.Property(st => st.ReferenceNo).HasMaxLength(100);
        supplierTransaction.Property(st => st.DebitAmount).HasPrecision(18, 2);
        supplierTransaction.Property(st => st.CreditAmount).HasPrecision(18, 2);
        supplierTransaction.Property(st => st.Balance).HasPrecision(18, 2);
        supplierTransaction.Property(st => st.Description).HasMaxLength(500);
        supplierTransaction.Property(st => st.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        supplierTransaction.Property(st => st.UpdatedAt).HasColumnType("datetime");
        supplierTransaction.Property(st => st.RowVersion)
                           .HasColumnType("timestamp")
                           .ValueGeneratedOnAddOrUpdate()
                           .IsConcurrencyToken();
        supplierTransaction.Property(st => st.IsActive);
        supplierTransaction.HasOne(st => st.Supplier)
                           .WithMany()
                           .HasForeignKey(st => st.SupplierId)
                           .OnDelete(DeleteBehavior.Restrict);
        supplierTransaction.HasIndex(st => st.SupplierId).HasDatabaseName("IX_SupplierTransactions_SupplierId");
        supplierTransaction.HasIndex(st => st.TransactionDate).HasDatabaseName("IX_SupplierTransactions_TransactionDate");
        supplierTransaction.HasIndex(st => st.TransactionType).HasDatabaseName("IX_SupplierTransactions_TransactionType");
        supplierTransaction.HasIndex(st => st.ReferenceId).HasDatabaseName("IX_SupplierTransactions_ReferenceId");

        // StockAdjustments
        var stockAdjustment = modelBuilder.Entity<StockAdjustment>();
        stockAdjustment.ToTable("StockAdjustments");
        stockAdjustment.HasKey(sa => sa.Id);
        stockAdjustment.Property(sa => sa.Id).HasColumnName("StockAdjustmentId");
        stockAdjustment.Ignore(sa => sa.StockAdjustmentId);
        stockAdjustment.Property(sa => sa.ReferenceNo).HasMaxLength(50).IsRequired();
        stockAdjustment.Property(sa => sa.AdjustmentDate).IsRequired();
        stockAdjustment.Property(sa => sa.AdjustedBy).HasMaxLength(100).IsRequired();
        stockAdjustment.Property(sa => sa.Reason).HasMaxLength(50).IsRequired();
        stockAdjustment.Property(sa => sa.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Draft");
        stockAdjustment.Property(sa => sa.Remarks).HasMaxLength(500);
        stockAdjustment.Property(sa => sa.ApprovedBy).HasMaxLength(100);
        stockAdjustment.Property(sa => sa.TotalValue).HasColumnType("decimal(18,2)");
        stockAdjustment.Property(sa => sa.IsActive).HasDefaultValue(true);
        stockAdjustment.Property(sa => sa.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        stockAdjustment.Property(sa => sa.RowVersion)
                       .HasColumnType("timestamp")
                       .ValueGeneratedOnAddOrUpdate()
                       .IsConcurrencyToken();
        stockAdjustment.HasIndex(sa => sa.ReferenceNo).IsUnique().HasDatabaseName("UX_StockAdjustments_ReferenceNo");
        stockAdjustment.HasIndex(sa => sa.AdjustmentDate).HasDatabaseName("IX_StockAdjustments_AdjustmentDate");
        stockAdjustment.HasIndex(sa => sa.Reason).HasDatabaseName("IX_StockAdjustments_Reason");
        stockAdjustment.HasIndex(sa => sa.Status).HasDatabaseName("IX_StockAdjustments_Status");

        // StockAdjustmentItems
        var stockAdjustmentItem = modelBuilder.Entity<StockAdjustmentItem>();
        stockAdjustmentItem.ToTable("StockAdjustmentItems");
        stockAdjustmentItem.HasKey(sai => sai.Id);
        stockAdjustmentItem.Property(sai => sai.Id).HasColumnName("StockAdjustmentItemId");
        stockAdjustmentItem.Ignore(sai => sai.StockAdjustmentItemId);
        stockAdjustmentItem.Property(sai => sai.BatchNo).HasMaxLength(50);
        stockAdjustmentItem.Property(sai => sai.Quantity).HasColumnType("decimal(12,3)");
        stockAdjustmentItem.Property(sai => sai.CurrentStock).HasColumnType("decimal(12,3)");
        stockAdjustmentItem.Property(sai => sai.CostPrice).HasColumnType("decimal(18,2)");
        stockAdjustmentItem.Property(sai => sai.TotalValue).HasColumnType("decimal(18,2)");
        stockAdjustmentItem.Property(sai => sai.Remarks).HasMaxLength(500);
        stockAdjustmentItem.Property(sai => sai.IsActive).HasDefaultValue(true);
        stockAdjustmentItem.Property(sai => sai.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        stockAdjustmentItem.Property(sai => sai.RowVersion)
                           .HasColumnType("timestamp")
                           .ValueGeneratedOnAddOrUpdate()
                           .IsConcurrencyToken();
        stockAdjustmentItem.HasOne(sai => sai.StockAdjustment)
                           .WithMany(sa => sa.Items)
                           .HasForeignKey(sai => sai.StockAdjustmentId)
                           .OnDelete(DeleteBehavior.Cascade);
        stockAdjustmentItem.HasOne(sai => sai.Product)
                           .WithMany()
                           .HasForeignKey(sai => sai.ProductId)
                           .OnDelete(DeleteBehavior.Restrict);
        stockAdjustmentItem.HasIndex(sai => sai.StockAdjustmentId).HasDatabaseName("IX_StockAdjustmentItems_StockAdjustmentId");
        stockAdjustmentItem.HasIndex(sai => sai.ProductId).HasDatabaseName("IX_StockAdjustmentItems_ProductId");

        // CashTransactions
        var cashTransaction = modelBuilder.Entity<CashTransaction>();
        cashTransaction.ToTable("CashTransactions");
        cashTransaction.HasKey(ct => ct.Id);
        cashTransaction.Property(ct => ct.Id).HasColumnName("CashTransactionId");
        cashTransaction.Property(ct => ct.TransactionDate).IsRequired().HasColumnType("datetime");
        cashTransaction.Property(ct => ct.Type).IsRequired().HasMaxLength(20);
        cashTransaction.Property(ct => ct.Amount).HasPrecision(18, 2);
        cashTransaction.Property(ct => ct.Description).HasMaxLength(500);
        cashTransaction.Property(ct => ct.ReferenceNo).HasMaxLength(50);
        cashTransaction.Property(ct => ct.Category).HasMaxLength(50);
        cashTransaction.Property(ct => ct.UserId).IsRequired();
        cashTransaction.Property(ct => ct.UserName).HasMaxLength(100);
        cashTransaction.Property(ct => ct.Remarks).HasMaxLength(500);
        cashTransaction.Property(ct => ct.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        cashTransaction.Property(ct => ct.UpdatedAt).HasColumnType("datetime");
        cashTransaction.Property(ct => ct.RowVersion)
                       .HasColumnType("timestamp")
                       .ValueGeneratedOnAddOrUpdate()
                       .IsConcurrencyToken();
        cashTransaction.Property(ct => ct.IsActive);
        cashTransaction.HasIndex(ct => ct.TransactionDate).HasDatabaseName("IX_CashTransactions_TransactionDate");
        cashTransaction.HasIndex(ct => ct.Type).HasDatabaseName("IX_CashTransactions_Type");
        cashTransaction.HasIndex(ct => ct.UserId).HasDatabaseName("IX_CashTransactions_UserId");
        modelBuilder.Entity<CashTransaction>().HasQueryFilter(ct => ct.IsActive);

        // CompanyProfiles
        var companyProfile = modelBuilder.Entity<CompanyProfile>();
        companyProfile.ToTable("CompanyProfiles");
        companyProfile.HasKey(cp => cp.Id);
        companyProfile.Property(cp => cp.Id).HasColumnName("CompanyProfileId").ValueGeneratedOnAdd();
        companyProfile.Property(cp => cp.Name).IsRequired().HasMaxLength(200);
        companyProfile.Property(cp => cp.Address).HasMaxLength(500);
        companyProfile.Property(cp => cp.City).HasMaxLength(100);
        companyProfile.Property(cp => cp.State).HasMaxLength(100);
        companyProfile.Property(cp => cp.PostalCode).HasMaxLength(20);
        companyProfile.Property(cp => cp.Country).HasMaxLength(100);
        companyProfile.Property(cp => cp.Phone).HasMaxLength(20);
        companyProfile.Property(cp => cp.Mobile).HasMaxLength(20);
        companyProfile.Property(cp => cp.Email).HasMaxLength(100);
        companyProfile.Property(cp => cp.Website).HasMaxLength(200);
        companyProfile.Property(cp => cp.GstNumber).HasMaxLength(50);
        companyProfile.Property(cp => cp.PanNumber).HasMaxLength(50);
        companyProfile.Property(cp => cp.LogoUrl).HasMaxLength(500);
        companyProfile.Property(cp => cp.CurrencySymbol).HasMaxLength(10).HasDefaultValue("₹");
        companyProfile.Property(cp => cp.CurrencyCode).HasMaxLength(10).HasDefaultValue("INR");
        companyProfile.Property(cp => cp.ReceiptHeader).HasMaxLength(100);
        companyProfile.Property(cp => cp.ReceiptFooter).HasMaxLength(200);
        companyProfile.Property(cp => cp.IsActive).HasDefaultValue(true);
        companyProfile.Property(cp => cp.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        companyProfile.Property(cp => cp.UpdatedAt).HasColumnType("datetime");

        // LoyaltySettings
        var loyaltySetting = modelBuilder.Entity<LoyaltySetting>();
        loyaltySetting.ToTable("LoyaltySettings");
        loyaltySetting.HasKey(ls => ls.Id);
        loyaltySetting.Property(ls => ls.Id).HasColumnName("LoyaltySettingId").ValueGeneratedOnAdd();
        loyaltySetting.Property(ls => ls.PointsPerUnitCurrency).HasPrecision(18, 4);
        loyaltySetting.Property(ls => ls.RedemptionValuePerPoint).HasPrecision(18, 4);
        loyaltySetting.Property(ls => ls.MinimumRedeemPoints);
        loyaltySetting.Property(ls => ls.IsActive).HasDefaultValue(true);
        loyaltySetting.Property(ls => ls.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        loyaltySetting.Property(ls => ls.UpdatedAt).HasColumnType("datetime");

        // CustomerTransactions
        var customerTransaction = modelBuilder.Entity<CustomerTransaction>();
        customerTransaction.ToTable("CustomerTransactions");
        customerTransaction.HasKey(ct => ct.Id);
        customerTransaction.Property(ct => ct.Id).HasColumnName("CustomerTransactionId");
        customerTransaction.Ignore(ct => ct.CustomerTransactionId);
        customerTransaction.Property(ct => ct.CustomerId).IsRequired();
        customerTransaction.Property(ct => ct.TransactionDate).IsRequired().HasColumnType("datetime");
        customerTransaction.Property(ct => ct.TransactionType).IsRequired().HasMaxLength(20);
        customerTransaction.Property(ct => ct.ReferenceId);
        customerTransaction.Property(ct => ct.ReferenceNo).HasMaxLength(100);
        customerTransaction.Property(ct => ct.DebitAmount).HasPrecision(18, 2);
        customerTransaction.Property(ct => ct.CreditAmount).HasPrecision(18, 2);
        customerTransaction.Property(ct => ct.Balance).HasPrecision(18, 2);
        customerTransaction.Property(ct => ct.Description).HasMaxLength(500);
        customerTransaction.Property(ct => ct.PaymentMode).HasMaxLength(20);
        customerTransaction.Property(ct => ct.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        customerTransaction.Property(ct => ct.UpdatedAt).HasColumnType("datetime");
        customerTransaction.Property(ct => ct.RowVersion)
                           .HasColumnType("timestamp")
                           .ValueGeneratedOnAddOrUpdate()
                           .IsConcurrencyToken();
        customerTransaction.Property(ct => ct.IsActive);
        customerTransaction.HasOne(ct => ct.Customer)
                           .WithMany()
                           .HasForeignKey(ct => ct.CustomerId)
                           .OnDelete(DeleteBehavior.Restrict);
        customerTransaction.HasIndex(ct => ct.CustomerId).HasDatabaseName("IX_CustomerTransactions_CustomerId");
        customerTransaction.HasIndex(ct => ct.TransactionDate).HasDatabaseName("IX_CustomerTransactions_TransactionDate");
        customerTransaction.HasIndex(ct => ct.TransactionType).HasDatabaseName("IX_CustomerTransactions_TransactionType");

        // Quotations
        var quotation = modelBuilder.Entity<Quotation>();
        quotation.ToTable("Quotations");
        quotation.HasKey(q => q.Id);
        quotation.Property(q => q.Id).HasColumnName("QuotationId");
        quotation.Ignore(q => q.QuotationId);
        quotation.Property(q => q.QuotationNumber).IsRequired().HasMaxLength(50);
        quotation.Property(q => q.QuotationDate).IsRequired().HasColumnType("datetime");
        quotation.Property(q => q.ValidUntil).HasColumnType("datetime");
        quotation.Property(q => q.CustomerName).HasMaxLength(200);
        quotation.Property(q => q.CustomerPhone).HasMaxLength(20);
        quotation.Property(q => q.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        quotation.Property(q => q.Subtotal).HasPrecision(18, 2);
        quotation.Property(q => q.DiscountAmount).HasPrecision(18, 2);
        quotation.Property(q => q.TaxAmount).HasPrecision(18, 2);
        quotation.Property(q => q.TotalAmount).HasPrecision(18, 2);
        quotation.Property(q => q.Notes).HasMaxLength(500);
        quotation.Property(q => q.TermsAndConditions).HasMaxLength(1000);
        quotation.Property(q => q.ConvertedAt).HasColumnType("datetime");
        quotation.Property(q => q.ConvertedBy).HasMaxLength(100);
        quotation.Property(q => q.CreatedBy).IsRequired().HasMaxLength(100);
        quotation.Property(q => q.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        quotation.Property(q => q.UpdatedAt).HasColumnType("datetime");
        quotation.Property(q => q.RowVersion)
                 .HasColumnType("timestamp")
                 .ValueGeneratedOnAddOrUpdate()
                 .IsConcurrencyToken();
        quotation.Property(q => q.IsActive).HasDefaultValue(true);
        quotation.HasOne(q => q.Customer)
                 .WithMany()
                 .HasForeignKey(q => q.CustomerId)
                 .OnDelete(DeleteBehavior.SetNull);
        quotation.HasMany(q => q.Items)
                 .WithOne(qi => qi.Quotation)
                 .HasForeignKey(qi => qi.QuotationId)
                 .OnDelete(DeleteBehavior.Cascade);
        quotation.HasIndex(q => q.QuotationNumber).IsUnique().HasDatabaseName("UX_Quotations_QuotationNumber");
        quotation.HasIndex(q => q.QuotationDate).HasDatabaseName("IX_Quotations_QuotationDate");
        quotation.HasIndex(q => q.CustomerId).HasDatabaseName("IX_Quotations_CustomerId");
        quotation.HasIndex(q => q.Status).HasDatabaseName("IX_Quotations_Status");
        modelBuilder.Entity<Quotation>().HasQueryFilter(q => q.IsActive);

        // QuotationItems
        var quotationItem = modelBuilder.Entity<QuotationItem>();
        quotationItem.ToTable("QuotationItems");
        quotationItem.HasKey(qi => qi.Id);
        quotationItem.Property(qi => qi.Id).HasColumnName("QuotationItemId");
        quotationItem.Ignore(qi => qi.QuotationItemId);
        quotationItem.Property(qi => qi.QuotationId).IsRequired();
        quotationItem.Property(qi => qi.ProductId).IsRequired();
        quotationItem.Property(qi => qi.ProductName).IsRequired().HasMaxLength(200);
        quotationItem.Property(qi => qi.SKU).IsRequired().HasMaxLength(100);
        quotationItem.Property(qi => qi.HSNCode).HasMaxLength(20);
        quotationItem.Property(qi => qi.Quantity).HasPrecision(18, 3);
        quotationItem.Property(qi => qi.UnitName).HasMaxLength(50);
        quotationItem.Property(qi => qi.UnitPrice).HasPrecision(18, 2);
        quotationItem.Property(qi => qi.DiscountPercent).HasPrecision(5, 2);
        quotationItem.Property(qi => qi.DiscountAmount).HasPrecision(18, 2);
        quotationItem.Property(qi => qi.TaxRate).HasPrecision(5, 2);
        quotationItem.Property(qi => qi.TaxAmount).HasPrecision(18, 2);
        quotationItem.Property(qi => qi.TotalAmount).HasPrecision(18, 2);
        quotationItem.Property(qi => qi.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        quotationItem.Property(qi => qi.UpdatedAt).HasColumnType("datetime");
        quotationItem.Property(qi => qi.RowVersion)
                     .HasColumnType("timestamp")
                     .ValueGeneratedOnAddOrUpdate()
                     .IsConcurrencyToken();
        quotationItem.Property(qi => qi.IsActive).HasDefaultValue(true);
        quotationItem.HasOne(qi => qi.Product)
                     .WithMany()
                     .HasForeignKey(qi => qi.ProductId)
                     .OnDelete(DeleteBehavior.Restrict);
        quotationItem.HasIndex(qi => qi.QuotationId).HasDatabaseName("IX_QuotationItems_QuotationId");
        quotationItem.HasIndex(qi => qi.ProductId).HasDatabaseName("IX_QuotationItems_ProductId");
        modelBuilder.Entity<QuotationItem>().HasQueryFilter(qi => qi.IsActive);

        // StockSummaries
        var stockSummary = modelBuilder.Entity<StockSummary>();
        stockSummary.ToTable("StockSummary"); // Using singular to match existing code references
        stockSummary.HasKey(ss => ss.ProductId);
        stockSummary.Property(ss => ss.ProductId).ValueGeneratedNever();
        stockSummary.Property(ss => ss.AvailableStock).HasPrecision(18, 3);
        stockSummary.Property(ss => ss.LastUpdated).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        stockSummary.HasOne<Product>()
                    .WithOne()
                    .HasForeignKey<StockSummary>(ss => ss.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
    }
}
