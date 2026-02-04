using POS.Domain.Enums;

namespace POS.Domain.Entities
{
    public class Payment
    {
        public long PaymentId { get; set; }
        public long SaleId { get; set; }
        public Sale? Sale { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? CardType { get; set; }
        public string? CardLastFour { get; set; }
        public string? UPIId { get; set; }
        public string? BankName { get; set; }
        public decimal? TenderedAmount { get; set; }
        public decimal? ChangeAmount { get; set; }
        public PaymentStatus Status { get; set; }
        public string? GatewayTransactionId { get; set; }
        public string? GatewayResponse { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
