namespace POS.Shared.Models;

public class BatchDto
{
    public Guid BatchId { get; set; }
    
    // Product Information
    public long ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSKU { get; set; }
    
    // Batch Details
    public string BatchNo { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public DateTime? ManufactureDate { get; set; }
    
    // Pricing
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal MRP { get; set; }
    
    // Stock Quantities
    public decimal ReceivedQuantity { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal AllocatedQuantity { get; set; }
    public decimal SoldQuantity { get; set; }
    public decimal ReturnedQuantity { get; set; }
    public decimal AdjustedQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    
    // Source
    public Guid? PurchaseEntryId { get; set; }
    public Guid? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    
    // Location
    public string? LocationCode { get; set; }
    public string? BinLocation { get; set; }
    
    // Dates
    public DateTime ReceivedDate { get; set; }
    public string? ReceivedBy { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    
    // Status
    public bool IsExpired { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
