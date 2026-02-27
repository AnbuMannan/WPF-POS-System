using System.ComponentModel.DataAnnotations.Schema;
using POS.Domain.Interfaces;

namespace POS.Domain.Entities;

/// <summary>
/// Stock adjustment header - tracks inventory adjustments due to damage, theft, expiry, or corrections
/// </summary>
public class StockAdjustment : BaseEntity, IStoreEntity
{
    public int StoreCode { get; set; }
    [NotMapped]
    public Guid StockAdjustmentId
    {
        get => Id;
        set => Id = value;
    }

    /// <summary>
    /// Auto-generated reference number (e.g., ADJ-2026-0001)
    /// </summary>
    public string ReferenceNo { get; set; } = string.Empty;

    /// <summary>
    /// Date of the adjustment
    /// </summary>
    public DateTime AdjustmentDate { get; set; } = DateTime.Now;

    /// <summary>
    /// User who performed the adjustment
    /// </summary>
    public string AdjustedBy { get; set; } = string.Empty;

    /// <summary>
    /// Reason for adjustment: Damage, Theft, Expiry, Correction, Other
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Status of the adjustment: Draft, Approved, Cancelled
    /// </summary>
    public string Status { get; set; } = "Draft";

    /// <summary>
    /// Additional remarks/notes
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// Date when the adjustment was approved/processed
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// User who approved the adjustment
    /// </summary>
    public string? ApprovedBy { get; set; }

    /// <summary>
    /// Total value of items adjusted (for reporting)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalValue { get; set; }

    // Navigation property
    public ICollection<StockAdjustmentItem> Items { get; set; } = new List<StockAdjustmentItem>();
}
