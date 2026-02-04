namespace POS.Domain.Entities
{
    public class PriceOverrideLog
    {
        public int LogId { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal OverriddenPrice { get; set; }
        public string? Reason { get; set; }
        public long SaleId { get; set; }
        public string OverriddenBy { get; set; } = string.Empty;
        public DateTime OverriddenAt { get; set; }
    }
}
