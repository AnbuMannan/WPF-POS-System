namespace POS.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

public class Uom : BaseEntity
{
    [NotMapped]
    public Guid UomId
    {
        get => Id;
        set => Id = value;
    }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Symbol { get; set; }
    public int DecimalPlaces { get; set; }
    public string? Description { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
