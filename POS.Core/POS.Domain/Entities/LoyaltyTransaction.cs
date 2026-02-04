namespace POS.Domain.Entities
{
    public class LoyaltyTransaction
    {
        public int TransactionId { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public long? SaleId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
