namespace POS.Shared.Models
{
    public class DraftBillDto
    {
        public int Id { get; set; }
        public string? BillNumber { get; set; }
        public string? DraftName { get; set; }
        public string CartData { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
