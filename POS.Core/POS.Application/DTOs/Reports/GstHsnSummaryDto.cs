namespace POS.Application.DTOs.Reports;

public class GstHsnSummaryDto
{
    public string HSNCode { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TaxableValue { get; set; }
    public decimal CGST { get; set; }
    public decimal SGST { get; set; }
    public decimal IGST { get; set; }
}
