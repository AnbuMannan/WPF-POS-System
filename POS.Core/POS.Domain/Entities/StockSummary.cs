namespace POS.Domain.Entities;

public class StockSummary
{
    public Guid ProductId { get; set; }
    public decimal AvailableStock { get; set; }
    public DateTime LastUpdated { get; set; }
}
