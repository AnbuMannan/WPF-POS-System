using POS.Domain.Enums;

namespace POS.Domain.Entities
{
    public class Sale
    {
        public long SaleId { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public string? InvoiceNumber { get; set; }
        public SaleType SaleType { get; set; }
        public SaleStatus Status { get; set; }
        public Guid? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerGSTIN { get; set; }
        public decimal Subtotal { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal Cess { get; set; }
        public decimal TotalTax { get; set; }
        public decimal RoundOff { get; set; }
        public decimal TotalAmount { get; set; }
        public string? CouponCode { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string? DraftName { get; set; }
        public string? Notes { get; set; }
        public string? TerminalId { get; set; }
        public bool IsDraft { get; set; }
        public bool IsHeld { get; set; }
        public DateTime? HeldAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? GiftCardId { get; set; }
        public int LoyaltyPointsEarned { get; set; }
        public int LoyaltyPointsRedeemed { get; set; }
        public bool IsLocked { get; set; }
        public string? LockedBy { get; set; }
        public DateTime? LockedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
