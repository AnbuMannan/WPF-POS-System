namespace POS.Shared.Models
{
    public class CompanyProfileDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? Phone { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? GstNumber { get; set; }
        public string? PanNumber { get; set; }
        public string? LogoUrl { get; set; }
        public string CurrencySymbol { get; set; } = "₹";
        public string CurrencyCode { get; set; } = "INR";
        public string? ReceiptHeader { get; set; }
        public string? ReceiptFooter { get; set; }
    }

    public class UpdateCompanyProfileDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? Phone { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? GstNumber { get; set; }
        public string? PanNumber { get; set; }
        public string? LogoUrl { get; set; }
        public string CurrencySymbol { get; set; } = "₹";
        public string CurrencyCode { get; set; } = "INR";
        public string? ReceiptHeader { get; set; }
        public string? ReceiptFooter { get; set; }
    }
}
