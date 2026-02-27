using System.ComponentModel.DataAnnotations;

namespace POS.AuthService.Entities
{
    public class Store
    {
        [Key]
        public int StoreCode { get; set; }
        [Required]
        [MaxLength(200)]
        public string StoreName { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Address { get; set; }
        [MaxLength(20)]
        public string? TaxId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}