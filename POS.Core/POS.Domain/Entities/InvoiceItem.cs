namespace POS.Domain.Entities;

public class InvoiceItem
{
    public Guid InvoiceItemId { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid ProductId { get; set; }
    public string HSNCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal CGST { get; set; }
    public decimal SGST { get; set; }
    public decimal IGST { get; set; }
    public decimal Total { get; set; }
}
