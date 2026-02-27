using POS.Domain.Interfaces;

namespace POS.Domain.Entities
{
    public class HeldBill : IStoreEntity
    {
        public long HeldBillId { get; set; }
        public int StoreCode { get; set; }
        public string HoldReference { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public Guid? CustomerId { get; set; }
        public string CartData { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string HeldBy { get; set; } = string.Empty;
        public DateTime HeldAt { get; set; }
        public string? RetrievedBy { get; set; }
        public DateTime? RetrievedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRetrieved { get; set; }
    }
}
