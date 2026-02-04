namespace POS.Domain.Entities
{
    public class GiftCardTransaction
    {
        public int TransactionId { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public int GiftCardId { get; set; }
        public GiftCard? GiftCard { get; set; }
        public long? SaleId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
