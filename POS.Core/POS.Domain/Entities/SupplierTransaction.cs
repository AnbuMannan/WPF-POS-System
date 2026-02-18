using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Domain.Entities;

/// <summary>
/// Supplier ledger entry - tracks all transactions with suppliers
/// </summary>
public class SupplierTransaction : BaseEntity
{
    [NotMapped]
    public Guid SupplierTransactionId
    {
        get => Id;
        set => Id = value;
    }

    /// <summary>
    /// Supplier involved in the transaction
    /// </summary>
    public Guid SupplierId { get; set; }

    /// <summary>
    /// Date of the transaction
    /// </summary>
    public DateTime TransactionDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Type of transaction: Purchase, PurchaseReturn, Payment, OpeningBalance
    /// </summary>
    public string TransactionType { get; set; } = string.Empty;

    /// <summary>
    /// Reference document ID (PurchaseEntry ID, PurchaseReturn ID, Payment ID)
    /// </summary>
    public Guid? ReferenceId { get; set; }

    /// <summary>
    /// Reference number for display (Invoice No, Return No, Payment No)
    /// </summary>
    public string? ReferenceNo { get; set; }

    /// <summary>
    /// Debit amount - Payment to supplier (reduces balance)
    /// </summary>
    public decimal DebitAmount { get; set; }

    /// <summary>
    /// Credit amount - Purchase from supplier (increases balance)
    /// </summary>
    public decimal CreditAmount { get; set; }

    /// <summary>
    /// Running balance after this transaction
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Description/Narration
    /// </summary>
    public string? Description { get; set; }

    // Navigation properties
    public virtual Supplier? Supplier { get; set; }
}
