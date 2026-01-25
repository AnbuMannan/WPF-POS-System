namespace POS.Application.DTOs.Reports;

public class GstDailyCollectionDto
{
    public DateTime Date { get; set; }
    public decimal CGST { get; set; }
    public decimal SGST { get; set; }
    public decimal IGST { get; set; }
    public decimal TotalGST => CGST + SGST + IGST;
}
