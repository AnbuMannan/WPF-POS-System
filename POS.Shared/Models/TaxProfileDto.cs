namespace POS.Shared.Models;

public class TaxProfileDto
{
    public int TaxProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal CGST { get; set; }
    public decimal SGST { get; set; }
    public decimal IGST { get; set; }
    public decimal Cess { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
