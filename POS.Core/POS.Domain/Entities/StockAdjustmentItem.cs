using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Domain.Entities;

/// <summary>
/// Stock adjustment line item - individual product adjustments
/// </summary>
public class StockAdjustmentItem : BaseEntity
{
    [NotMapped]
    public Guid StockAdjustmentItemId
    {
        get => Id;
        set => Id = value;
    }

    public Guid StockAdjustmentId { get; set; }

    [ForeignKey(nameof(StockAdjustmentId))]
    public StockAdjustment StockAdjustment { get; set; } = null!;

    public long ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = null!;

    /// <summary>
    /// Optional batch number for batch-tracked products
    /// </summary>
    public string? BatchNo { get; set; }

    /// <summary>
    /// Quantity being adjusted (positive = increase, negative = decrease)
    /// For Damage/Theft/Expiry: typically negative (stock out)
    /// For Correction: can be positive or negative
    /// </summary>
    [Column(TypeName = "decimal(12,3)")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Current stock at time of adjustment (for reference)
    /// </summary>
    [Column(TypeName = "decimal(12,3)")]
    public decimal CurrentStock { get; set; }

    /// <summary>
    /// Cost price per unit at time of adjustment
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal CostPrice { get; set; }

    /// <summary>
    /// Total value of this adjustment line (Quantity * CostPrice)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalValue { get; set; }

    /// <summary>
    /// Optional item-level remarks
    /// </summary>
    public string? Remarks { get; set; }
}
