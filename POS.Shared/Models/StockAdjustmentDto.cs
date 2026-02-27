namespace POS.Shared.Models;

/// <summary>
/// DTO for stock adjustment header
/// </summary>
public class StockAdjustmentDto
{
    public Guid Id { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    public string AdjustedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public decimal TotalValue { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<StockAdjustmentItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for stock adjustment line item
/// </summary>
public class StockAdjustmentItemDto
{
    public Guid Id { get; set; }
    public Guid StockAdjustmentId { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSku { get; set; }
    public string? BatchNo { get; set; }
    public decimal Quantity { get; set; }
    public decimal CurrentStock { get; set; }
    public decimal CostPrice { get; set; }
    public decimal TotalValue { get; set; }
    public string? Remarks { get; set; }
}

/// <summary>
/// DTO for creating a new stock adjustment
/// </summary>
public class CreateStockAdjustmentDto
{
    public DateTime AdjustmentDate { get; set; } = DateTime.Now;
    public string AdjustedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Remarks { get; set; }

    public List<CreateStockAdjustmentItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for creating a stock adjustment line item
/// </summary>
public class CreateStockAdjustmentItemDto
{
    public long ProductId { get; set; }
    public string? BatchNo { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public string? Remarks { get; set; }
}

/// <summary>
/// Available adjustment reasons
/// </summary>
public static class AdjustmentReasons
{
    public const string Damage = "Damage";
    public const string Theft = "Theft";
    public const string Expiry = "Expiry";
    public const string Correction = "Correction";
    public const string Other = "Other";
    public const string Audit = "Physical Stock Audit";

    public static readonly string[] All = { Damage, Theft, Expiry, Correction, Other, Audit };
}

/// <summary>
/// Adjustment status values
/// </summary>
public static class AdjustmentStatus
{
    public const string Draft = "Draft";
    public const string Approved = "Approved";
    public const string Cancelled = "Cancelled";
}
