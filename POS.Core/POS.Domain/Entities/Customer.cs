using System.ComponentModel.DataAnnotations;

namespace POS.Domain.Entities;

public class Customer
{
    [Key]
    [MaxLength(36)]
    public string CustomerId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(50)]
    public string? LoyaltyNumber { get; set; }

    [Required]
    public bool IsWholesale { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    /// <summary>Concurrency token (MySQL TIMESTAMP). Stored as DateTime to avoid EF Core/Pomelo byte[] mapping NRE.</summary>
    public DateTime RowVersion { get; set; }
}
