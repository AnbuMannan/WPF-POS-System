using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Domain.Entities;

/// <summary>
/// Represents a payment made to a supplier
/// </summary>
public class SupplierPayment : BaseEntity
{
    [NotMapped]
    public Guid SupplierPaymentId
    {
        get => Id;
        set => Id = value;
    }

    /// <summary>
    /// Supplier to whom payment is made
    /// </summary>
    public Guid SupplierId { get; set; }

    /// <summary>
    /// Date of the payment
    /// </summary>
    public DateTime PaymentDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Payment amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Mode of payment: Cash, Bank, Cheque, UPI, Card
    /// </summary>
    public string PaymentMode { get; set; } = "Cash";

    /// <summary>
    /// Reference number (Cheque No, Transaction ID, etc.)
    /// </summary>
    public string? ReferenceNo { get; set; }

    /// <summary>
    /// Bank name (for Bank/Cheque payments)
    /// </summary>
    public string? BankName { get; set; }

    /// <summary>
    /// Additional remarks
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// Auto-generated payment voucher number
    /// </summary>
    public string PaymentNo { get; set; } = string.Empty;

    // Navigation properties
    public virtual Supplier? Supplier { get; set; }
}
