namespace POS.Shared.Models;

public class CustomerDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? LoyaltyNumber { get; set; }
    public bool IsWholesale { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Concurrency token (MySQL TIMESTAMP).</summary>
    public byte[]? RowVersion { get; set; }

    public string FullName => $"{FirstName ?? ""} {LastName ?? ""}".Trim();
}
