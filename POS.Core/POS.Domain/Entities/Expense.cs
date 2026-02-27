using POS.Domain.Interfaces;

namespace POS.Domain.Entities;

public class Expense : BaseEntity, IStoreEntity
{
    public int StoreCode { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
