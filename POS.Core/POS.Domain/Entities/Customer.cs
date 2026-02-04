using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Domain.Entities;

public class Customer : BaseEntity
{
    [NotMapped]
    public Guid CustomerId
    {
        get => Id;
        set => Id = value;
    }

    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public int LoyaltyPoints { get; set; }
}
