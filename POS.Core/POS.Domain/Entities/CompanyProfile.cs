using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Domain.Entities
{
    public class CompanyProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(20)]
        public string? Mobile { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? Website { get; set; }

        [MaxLength(50)]
        public string? GstNumber { get; set; }

        [MaxLength(50)]
        public string? PanNumber { get; set; }

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [MaxLength(10)]
        public string CurrencySymbol { get; set; } = "₹";

        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "INR";

        [MaxLength(100)]
        public string? ReceiptHeader { get; set; }

        [MaxLength(200)]
        public string? ReceiptFooter { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
