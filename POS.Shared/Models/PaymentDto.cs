namespace POS.Shared.Models
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // Cash, Card, UPI, etc.
        public decimal Amount { get; set; }
        public string? ReferenceNumber { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
