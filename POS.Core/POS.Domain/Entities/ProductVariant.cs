namespace POS.Domain.Entities;

public class ProductVariant
{
    public Guid VariantId { get; set; }
    public Guid ProductId { get; set; }
    public string VariantName { get; set; }
    public string Barcode { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal MRP { get; set; }
    public bool IsActive { get; set; }
}
