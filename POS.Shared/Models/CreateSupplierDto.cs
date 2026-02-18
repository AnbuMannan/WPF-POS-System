namespace POS.Shared.Models;

public class CreateSupplierDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? GstVatNumber { get; set; }
    public int? CreditPeriodDays { get; set; }
    public decimal? CreditLimit { get; set; }
}
