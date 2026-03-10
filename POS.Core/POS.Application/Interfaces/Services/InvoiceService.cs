using POS.Application.Common;
using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using TaxCalculator = POS.Application.Common.TaxCalculator;

namespace POS.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;

    public InvoiceService(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<Guid> CreateInvoiceAsync(Invoice invoice)
    {
        invoice.InvoiceId = Guid.NewGuid();
        invoice.InvoiceDate = DateTime.Now;

        decimal totalTax = 0;
        decimal totalAmount = 0;

        foreach (var item in invoice.Items)
        {
            item.InvoiceItemId = Guid.NewGuid();
            item.InvoiceId = invoice.InvoiceId;

            var baseAmount = item.Rate * item.Quantity;
            item.BaseAmount = baseAmount;

            var tax = TaxCalculator.CalculateTax(
                baseAmount,
                item.TaxPercent,
                false // tax-exclusive assumed, can be dynamic later
            );

            var (cgst, sgst, igst) = TaxCalculator.SplitGST(tax, invoice.IsInterState);

            item.CGST = cgst;
            item.SGST = sgst;
            item.IGST = igst;
            item.Total = baseAmount + tax;

            totalTax += tax;
            totalAmount += item.Total;
        }

        invoice.TotalTax = totalTax;
        invoice.TotalAmount = totalAmount;

        await _invoiceRepository.SaveAsync(invoice);
        return invoice.InvoiceId;
    }
}
