namespace POS.Domain.Entities;

public class TaxProfile
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal CGST { get; set; }
    public decimal SGST { get; set; }
    public decimal IGST { get; set; }
    public decimal Cess { get; set; }
    public bool IsActive { get; set; }
}
