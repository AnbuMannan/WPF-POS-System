namespace POS.Shared.Models
{
    public class HeldBillDto
    {
        public int Id { get; set; }
        public string HoldReason { get; set; } = string.Empty;
        public string CartData { get; set; } = string.Empty;
        public DateTime HeldAt { get; set; }
        public string? HeldBy { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
