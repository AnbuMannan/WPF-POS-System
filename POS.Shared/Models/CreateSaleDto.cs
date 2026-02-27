namespace POS.Shared.Models
{
    public class CreateSaleDto
    {
        public string? BillNumber { get; set; }
        public Guid? CustomerId { get; set; }
        public List<SaleItemDto> Items { get; set; } = new();
        public List<PaymentDto> Payments { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public int LoyaltyPointsRedeemed { get; set; }
        public decimal LoyaltyRedemptionAmount { get; set; }
    }

    public class SaleItemDto
    {
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? SKU { get; set; }
        public string? Unit { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? HSNCode { get; set; }
        /// <summary>Tax profile ID for FK. If 0 or not set, backend will use first available profile.</summary>
        public int TaxProfileId { get; set; }
    }
}
