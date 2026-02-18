using Dapper;
using POS.Application.Interfaces.Repositories;
using POS.Shared.Models;
using System.Data;

namespace POS.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly IDbConnection _db;

    public ReportRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<List<SalesSummaryReportRow>> GetSalesReportAsync(DateTime from, DateTime to, Guid? customerId, string? status)
    {
        var sql = @"SELECT DATE(s.CreatedAt) AS Date,
                           COUNT(*) AS InvoiceCount,
                           SUM(s.TotalAmount) AS TotalAmount,
                           SUM(s.Subtotal) AS Subtotal,
                           SUM(s.TotalTax) AS TaxAmount
                    FROM Sales s
                    WHERE s.CreatedAt >= @from AND s.CreatedAt < @to
                      AND (@customerId IS NULL OR s.CustomerId = @customerId)
                      AND (@status IS NULL OR s.Status = @status)
                    GROUP BY DATE(s.CreatedAt)
                    ORDER BY DATE(s.CreatedAt)";

        var rows = await _db.QueryAsync<SalesSummaryReportRow>(sql, new { from, to, customerId, status });
        return rows.ToList();
    }

    public async Task<List<ItemWiseSalesRow>> GetItemWiseSalesAsync(DateTime from, DateTime to, int? categoryId)
    {
        var sql = @"SELECT si.ProductId,
                           p.Name AS ProductName,
                           c.Name AS CategoryName,
                           SUM(si.Quantity) AS QuantitySold,
                           SUM(si.TotalAmount) AS TotalAmount
                    FROM SaleItems si
                    JOIN Sales s ON s.SaleId = si.SaleId
                    JOIN Products p ON p.ProductId = si.ProductId
                    LEFT JOIN Categories c ON c.CategoryId = p.CategoryId
                    WHERE s.CreatedAt >= @from AND s.CreatedAt < @to
                      AND s.Status = 'Completed'
                      AND s.IsDraft = 0
                      AND s.IsHeld = 0
                      AND (@categoryId IS NULL OR p.CategoryId = @categoryId)
                    GROUP BY si.ProductId, p.Name, c.Name
                    ORDER BY QuantitySold DESC";

        var rows = await _db.QueryAsync<ItemWiseSalesRow>(sql, new { from, to, categoryId });
        return rows.ToList();
    }

    public async Task<ProfitLossReportDto> GetProfitLossReportAsync(DateTime from, DateTime to)
    {
        var revenueSql = @"SELECT COALESCE(SUM(TotalAmount),0) 
                           FROM Sales 
                           WHERE CreatedAt >= @from AND CreatedAt < @to
                             AND Status = 'Completed'
                             AND IsDraft = 0
                             AND IsHeld = 0";

        var cogsSql = @"SELECT COALESCE(SUM(Quantity * CostPrice),0)
                        FROM SaleItems si
                        JOIN Products p ON p.ProductId = si.ProductId
                        JOIN Sales s ON s.SaleId = si.SaleId
                        WHERE s.CreatedAt >= @from AND s.CreatedAt < @to
                          AND s.Status = 'Completed'
                          AND s.IsDraft = 0
                          AND s.IsHeld = 0";

        var expensesSql = @"SELECT COALESCE(SUM(Amount),0)
                            FROM CashTransactions
                            WHERE TransactionDate >= @from AND TransactionDate < @to
                              AND Type = 'Expense'";

        var revenue = await _db.ExecuteScalarAsync<decimal>(revenueSql, new { from, to });
        var cogs = await _db.ExecuteScalarAsync<decimal>(cogsSql, new { from, to });
        var expenses = await _db.ExecuteScalarAsync<decimal>(expensesSql, new { from, to });

        return new ProfitLossReportDto
        {
            From = from,
            To = to,
            TotalSales = revenue,
            TotalCogs = cogs,
            TotalExpenses = expenses,
            ProfitLoss = revenue - cogs - expenses
        };
    }

    public async Task<List<LowStockItemRow>> GetLowStockReportAsync(decimal threshold)
    {
        var sql = @"SELECT p.ProductId,
                           p.Name AS ProductName,
                           p.SKU,
                           COALESCE(SUM(b.CurrentQuantity),0) AS AvailableStock,
                           0 AS ReorderLevel
                    FROM Products p
                    LEFT JOIN Batches b ON b.ProductId = p.ProductId AND b.IsActive = 1
                    GROUP BY p.ProductId, p.Name, p.SKU
                    HAVING AvailableStock < @threshold
                    ORDER BY AvailableStock ASC";

        var rows = await _db.QueryAsync<LowStockItemRow>(sql, new { threshold });
        return rows.ToList();
    }
}
