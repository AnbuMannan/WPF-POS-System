namespace POS.Shared.Models
{
    public class StoreDto
    {
        public int StoreCode { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ContactNumber { get; set; }
        public string? TaxId { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateStoreDto
    {
        public string StoreName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ContactNumber { get; set; }
        public string? TaxId { get; set; }
    }
}