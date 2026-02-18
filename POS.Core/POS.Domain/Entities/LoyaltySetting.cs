using System.ComponentModel.DataAnnotations;

namespace POS.Domain.Entities;

public class LoyaltySetting
{
    [Key]
    public int Id { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PointsPerUnitCurrency { get; set; }

    [Range(0, double.MaxValue)]
    public decimal RedemptionValuePerPoint { get; set; }

    [Range(0, int.MaxValue)]
    public int MinimumRedeemPoints { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

