using System.ComponentModel.DataAnnotations.Schema;
using POS.Domain.Enums;
using POS.Domain.Interfaces;

namespace POS.Domain.Entities;

public class Quotation : BaseEntity, IStoreEntity
{
    public int StoreCode { get; set; }
    [NotMapped]
    public Guid QuotationId
    {
        get => Id;
        set => Id = value;
    }

    public string QuotationNumber { get; set; } = string.Empty;
    public DateTime QuotationDate { get; set; } = DateTime.Now;
    public DateTime? ValidUntil { get; set; }

    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }

    public QuotationStatus Status { get; set; } = QuotationStatus.Open;

    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }

    /// <summary>
    /// SaleId after conversion
    /// </summary>
    public long? ConvertedSaleId { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public string? ConvertedBy { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public ICollection<QuotationItem> Items { get; set; } = new List<QuotationItem>();
}
