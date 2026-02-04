namespace POS.Domain.Entities
{
    public class SaleReturn
    {
        public int ReturnId { get; set; }
        public string ReturnNumber { get; set; } = string.Empty;
        public string ReturnType { get; set; } = string.Empty;
        public decimal TotalReturnAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public string? Reason { get; set; }
        public long OriginalSaleId { get; set; }
        public Sale? OriginalSale { get; set; }
        public long? NewSaleId { get; set; }
        public Sale? NewSale { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public ICollection<ReturnItem> ReturnItems { get; set; } = new List<ReturnItem>();
    }
}
