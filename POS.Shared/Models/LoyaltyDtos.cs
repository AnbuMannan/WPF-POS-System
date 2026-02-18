namespace POS.Shared.Models
{
    public class LoyaltySettingsDto
    {
        public int Id { get; set; }
        public decimal PointsPerUnitCurrency { get; set; }
        public decimal RedemptionValuePerPoint { get; set; }
        public int MinimumRedeemPoints { get; set; }
    }

    public class UpdateLoyaltySettingsDto
    {
        public decimal PointsPerUnitCurrency { get; set; }
        public decimal RedemptionValuePerPoint { get; set; }
        public int MinimumRedeemPoints { get; set; }
    }

    public class RedeemPointsRequest
    {
        public Guid CustomerId { get; set; }
        public int PointsToRedeem { get; set; }
    }

    public class RedeemPointsResponse
    {
        public int PointsRedeemed { get; set; }
        public decimal RedemptionAmount { get; set; }
        public int RemainingPoints { get; set; }
    }
}

