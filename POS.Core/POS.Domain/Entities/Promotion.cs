using POS.Domain.Enums;
using POS.Domain.Interfaces;

namespace POS.Domain.Entities;

public class Promotion : BaseEntity, IStoreEntity
{
    public int StoreCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public PromotionType Type { get; set; }
    public decimal Value { get; set; }
    public decimal? MinPurchaseAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
