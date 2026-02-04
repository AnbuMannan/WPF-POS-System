namespace POS.Domain.Entities
{
    public class GiftCard
    {
        public int GiftCardId { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string? PIN { get; set; }
        public decimal InitialBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public DateTime ValidFrom { get; set; }
        public Guid? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
