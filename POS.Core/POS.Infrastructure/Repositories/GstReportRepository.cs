using Dapper;
using POS.Application.DTOs.Reports;
using POS.Application.Interfaces.Repositories;
using System.Data;

namespace POS.Infrastructure.Repositories;

public class GstReportRepository : IGstReportRepository
{
    private readonly IDbConnection _db;

    public GstReportRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<GstSummaryDto> GetGstSummaryAsync(DateTime from, DateTime to)
    {
        return await _db.QueryFirstAsync<GstSummaryDto>(@"
        SELECT 
          SUM(BaseAmount) AS TaxableValue,
          SUM(CGST) AS CGST,
          SUM(SGST) AS SGST,
          SUM(IGST) AS IGST
        FROM InvoiceItems II
        JOIN Invoices I ON I.InvoiceId = II.InvoiceId
        WHERE I.InvoiceDate BETWEEN @from AND @to",
        new { from, to });
    }

    public async Task<List<GstHsnSummaryDto>> GetHsnSummaryAsync(DateTime from, DateTime to)
    {
        var result = await _db.QueryAsync<GstHsnSummaryDto>(@"
        SELECT 
          II.HSNCode,
          II.TaxPercent,
          SUM(II.Quantity) AS TotalQuantity,
          SUM(II.BaseAmount) AS TaxableValue,
          SUM(II.CGST) AS CGST,
          SUM(II.SGST) AS SGST,
          SUM(II.IGST) AS IGST
        FROM InvoiceItems II
        JOIN Invoices I ON I.InvoiceId = II.InvoiceId
        WHERE I.InvoiceDate BETWEEN @from AND @to
        GROUP BY II.HSNCode, II.TaxPercent",
        new { from, to });

        return result.ToList();
    }

    public async Task<List<GstDailyCollectionDto>> GetDailyCollectionAsync(DateTime from, DateTime to)
    {
        var result = await _db.QueryAsync<GstDailyCollectionDto>(@"
        SELECT 
          DATE(I.InvoiceDate) AS Date,
          SUM(II.CGST) AS CGST,
          SUM(II.SGST) AS SGST,
          SUM(II.IGST) AS IGST
        FROM InvoiceItems II
        JOIN Invoices I ON I.InvoiceId = II.InvoiceId
        WHERE I.InvoiceDate BETWEEN @from AND @to
        GROUP BY DATE(I.InvoiceDate)",
        new { from, to });

        return result.ToList();
    }
}
