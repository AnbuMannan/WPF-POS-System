namespace POS.Shared.Models
{
    public class CartDataDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public Guid? CustomerId { get; set; }
        public string? DiscountInputValue { get; set; }
        public bool IsDiscountByPercent { get; set; }
    }

    public class CartItemDto
    {
        public Guid ProductId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal ActualPrice { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal TaxRate { get; set; }
    }
}
