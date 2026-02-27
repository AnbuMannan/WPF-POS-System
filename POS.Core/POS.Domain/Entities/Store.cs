using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Domain.Entities
{
    public class Store
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int StoreCode { get; set; }
        [Required]
        [MaxLength(200)]
        public string StoreName { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Address { get; set; }
        [MaxLength(20)]
        public string? ContactNumber { get; set; }
        [MaxLength(20)]
        public string? TaxId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}