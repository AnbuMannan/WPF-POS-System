using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Domain.Entities
{
    public class CashTransaction : BaseEntity
    {
        [Required]
        public DateTime TransactionDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = string.Empty; // CashIn, CashOut

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? ReferenceNo { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; } // Petty Cash, Expense, Opening Balance, etc.

        [Required]
        public int UserId { get; set; }

        [MaxLength(100)]
        public string? UserName { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }
    }
}
