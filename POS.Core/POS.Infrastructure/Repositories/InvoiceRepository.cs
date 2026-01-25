using Dapper;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using System.Data;

namespace POS.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly IDbConnection _db;

    public InvoiceRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task SaveAsync(Invoice invoice)
    {
        await _db.ExecuteAsync(@"
        INSERT INTO Invoices
        (InvoiceId,InvoiceNumber,InvoiceDate,IsInterState,TotalAmount,TotalTax)
        VALUES
        (@InvoiceId,@InvoiceNumber,@InvoiceDate,@IsInterState,@TotalAmount,@TotalTax)", invoice);

        foreach (var item in invoice.Items)
        {
            await _db.ExecuteAsync(@"
            INSERT INTO InvoiceItems
            (InvoiceItemId,InvoiceId,ProductId,HSNCode,Quantity,Rate,BaseAmount,TaxPercent,CGST,SGST,IGST,Total)
            VALUES
            (@InvoiceItemId,@InvoiceId,@ProductId,@HSNCode,@Quantity,@Rate,@BaseAmount,@TaxPercent,@CGST,@SGST,@IGST,@Total)", item);
        }
    }
}
