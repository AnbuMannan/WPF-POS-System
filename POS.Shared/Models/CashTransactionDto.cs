namespace POS.Shared.Models
{
    public class CashTransactionDto
    {
        public Guid Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Type { get; set; } = string.Empty; // CashIn, CashOut
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Category { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCashTransactionDto
    {
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Remarks { get; set; }
    }

    public class CashSummaryDto
    {
        public decimal TotalCashIn { get; set; }
        public decimal TotalCashOut { get; set; }
        public decimal CurrentBalance { get; set; }
        public int TransactionCount { get; set; }
        public DateTime? LastTransactionDate { get; set; }
    }
}
