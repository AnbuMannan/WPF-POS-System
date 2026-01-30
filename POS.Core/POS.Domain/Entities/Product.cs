using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Domain.Entities;

public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ProductId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string SKU { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Barcode { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;

    public int? BrandId { get; set; }

    [ForeignKey(nameof(BrandId))]
    public Brand? Brand { get; set; }

    [Required]
    public int TaxProfileId { get; set; }

    [ForeignKey(nameof(TaxProfileId))]
    public TaxProfile TaxProfile { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;

    [Column(TypeName = "decimal(12,2)")]
    public decimal CostPrice { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal SellingPrice { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal MRP { get; set; }

    [MaxLength(20)]
    public string? HSNCode { get; set; }

    public bool IsWeighable { get; set; }
    public bool IsManufactured { get; set; }
    public bool IsActive { get; set; } = true;

    [MaxLength(50)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>Concurrency token (MySQL TIMESTAMP). Stored as DateTime to avoid EF Core/Pomelo byte[] mapping NRE.</summary>
    public DateTime RowVersion { get; set; }
}
