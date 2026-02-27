using POS.Domain.Enums;
using POS.Domain.Interfaces;

namespace POS.Domain.Entities;

public class DeliveryOrder : BaseEntity, IStoreEntity
{
    public int StoreCode { get; set; }
    public long SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public string CustomerName { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public DeliveryStatus Status { get; set; }
    public DateTime? DispatchDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
}
