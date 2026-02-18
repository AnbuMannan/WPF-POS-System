using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Domain.Entities;

public class QuotationItem : BaseEntity
{
    [NotMapped]
    public Guid QuotationItemId
    {
        get => Id;
        set => Id = value;
    }

    public Guid QuotationId { get; set; }
    public Quotation Quotation { get; set; } = null!;

    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? HSNCode { get; set; }

    public decimal Quantity { get; set; }
    public string UnitName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
