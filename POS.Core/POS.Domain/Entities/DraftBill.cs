namespace POS.Domain.Entities
{
    public class DraftBill
    {
        public long DraftBillId { get; set; }
        public string DraftName { get; set; } = string.Empty;
        public string CartData { get; set; } = string.Empty;
        public Guid? CustomerId { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
