namespace POS.Shared.Models
{
    public class ExpenseDto
    {
        public Guid Id { get; set; }
        public int StoreCode { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}
