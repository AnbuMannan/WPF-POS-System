namespace POS.Shared.Models;

/// <summary>
/// DTO for supplier payment data
/// </summary>
public class SupplierPaymentDto
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierCode { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMode { get; set; } = "Cash";
    public string? ReferenceNo { get; set; }
    public string? BankName { get; set; }
    public string? Remarks { get; set; }
    public string PaymentNo { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating/updating supplier payment
/// </summary>
public class CreateSupplierPaymentDto
{
    public Guid SupplierId { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.Now;
    public decimal Amount { get; set; }
    public string PaymentMode { get; set; } = "Cash";
    public string? ReferenceNo { get; set; }
    public string? BankName { get; set; }
    public string? Remarks { get; set; }
}
