namespace POS.LicenseServer.Entities
{
    public class CentralLicense
    {
        public string LicenseKey { get; set; }
        public bool IsActivated { get; set; }
        public DateTime? ActivatedOn { get; set; }
        public string MachineId { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? TaxId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
    }

}
