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
        saleReturn.HasOne(r => r.OriginalSale).WithMany().HasForeignKey(r => r.OriginalSaleId).OnDelete(DeleteBehavior.Restrict);
        saleReturn.HasOne(r => r.NewSale).WithMany().HasForeignKey(r => r.NewSaleId).OnDelete(DeleteBehavior.SetNull);
        saleReturn.HasIndex(r => r.ReturnNumber).IsUnique().HasDatabaseName("uk_returns_returnnumber");
        saleReturn.HasIndex(r => r.OriginalSaleId).HasDatabaseName("idx_returns_originalsaleid");

        var returnItem = modelBuilder.Entity<ReturnItem>();
        returnItem.ToTable("ReturnItems");
        returnItem.HasKey(ri => ri.ReturnItemId);
        returnItem.Property(ri => ri.QuantityReturned).HasPrecision(18, 3);
        returnItem.Property(ri => ri.ReturnAmount).HasPrecision(18, 2);
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
    }
}
