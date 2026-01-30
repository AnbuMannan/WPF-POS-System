using System.ComponentModel.DataAnnotations;

namespace POS.Domain.Entities;

public class TaxProfile
{
    [Key]
    public int TaxProfileId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public decimal CGST { get; set; }

    [Required]
    public decimal SGST { get; set; }

    [Required]
    public decimal IGST { get; set; }

    [Required]
    public decimal Cess { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}
