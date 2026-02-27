using POS.Domain.Enums;
using POS.Domain.Interfaces;

namespace POS.Domain.Entities
{
    public class Payment : IStoreEntity
    {
        public int StoreCode { get; set; } = 1;
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
