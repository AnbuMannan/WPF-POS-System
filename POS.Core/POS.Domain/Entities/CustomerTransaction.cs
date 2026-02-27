using System.ComponentModel.DataAnnotations.Schema;
using POS.Domain.Interfaces;

namespace POS.Domain.Entities;

/// <summary>
/// Customer ledger entry - tracks all transactions with customers (Sale, Payment, Return, CreditNote)
/// </summary>
public class CustomerTransaction : BaseEntity, IStoreEntity
{
    public int StoreCode { get; set; }
    [NotMapped]
    public Guid CustomerTransactionId
    {
        get => Id;
        set => Id = value;
    }

    public Guid CustomerId { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Type: Sale, Payment, Return, CreditNote, OpeningBalance
    /// </summary>
    public string TransactionType { get; set; } = string.Empty;

    /// <summary>
    /// Reference document ID (SaleId, ReturnId, PaymentId)
    /// </summary>
    public Guid? ReferenceId { get; set; }

    /// <summary>
    /// Reference number for display (Invoice No, Return No, Payment No)
    /// </summary>
    public string? ReferenceNo { get; set; }

    /// <summary>
    /// Debit amount - Sale to customer (increases receivable)
    /// </summary>
    public decimal DebitAmount { get; set; }

    /// <summary>
    /// Credit amount - Payment/Return from customer (decreases receivable)
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

    /// <summary>
    /// Payment mode (Cash/Card/UPI/CreditNote) - for payment transactions
    /// </summary>
    public string? PaymentMode { get; set; }

    // Navigation properties
    public virtual Customer? Customer { get; set; }
}
