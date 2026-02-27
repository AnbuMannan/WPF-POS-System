namespace POS.Shared.Models;

public class ProductDto
{
    public long ProductId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? Description { get; set; }

    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? BrandId { get; set; }
    public string? BrandName { get; set; }

    public string Unit { get; set; } = string.Empty;

    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal MRP { get; set; }

    public string? HSNCode { get; set; }
    public int TaxProfileId { get; set; }

    public bool IsWeighable { get; set; }
    public bool IsManufactured { get; set; }
    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Concurrency token (MySQL TIMESTAMP).</summary>
    public byte[]? RowVersion { get; set; }

    // Extension properties for UI
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public decimal AvailableStock { get; set; }
}
