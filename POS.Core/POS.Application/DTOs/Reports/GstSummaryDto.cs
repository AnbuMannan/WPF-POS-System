namespace POS.Application.DTOs.Reports;

public class GstSummaryDto
{
    public decimal TaxableValue { get; set; }
    public decimal CGST { get; set; }
    public decimal SGST { get; set; }
    public decimal IGST { get; set; }
    public decimal TotalGST => CGST + SGST + IGST;
}
