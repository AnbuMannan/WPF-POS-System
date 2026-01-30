using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Domain.Entities;

public class Category
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public int? ParentCategoryId { get; set; }

    [ForeignKey(nameof(ParentCategoryId))]
    public Category? ParentCategory { get; set; }

    public ICollection<Category> Children { get; set; } = new List<Category>();

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(150)]
    public string? Slug { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public int Level { get; set; } = 1;

    [MaxLength(20)]
    public string? HSNCode { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(50)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
