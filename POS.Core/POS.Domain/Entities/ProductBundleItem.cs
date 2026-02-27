using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Domain.Entities;

public class ProductBundleItem : BaseEntity
{
    public long ParentProductId { get; set; }
    [ForeignKey(nameof(ParentProductId))]
    public Product ParentProduct { get; set; } = null!;
    public long ChildProductId { get; set; }
    [ForeignKey(nameof(ChildProductId))]
    public Product ChildProduct { get; set; } = null!;
    public decimal Quantity { get; set; }
}
