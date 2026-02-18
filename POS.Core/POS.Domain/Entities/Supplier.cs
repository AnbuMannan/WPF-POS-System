using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Domain.Entities;

public class Supplier : BaseEntity
{
    [NotMapped]
    public Guid SupplierId
    {
        get => Id;
        set => Id = value;
    }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? GstVatNumber { get; set; }
    public int? CreditPeriodDays { get; set; }
    public decimal? CreditLimit { get; set; }
}
