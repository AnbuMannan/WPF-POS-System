using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface IReportService
{
    Task<List<SalesSummaryReportRow>> GetSalesReportAsync(DateTime from, DateTime to, Guid? customerId, string? status);
    Task<List<ItemWiseSalesRow>> GetItemWiseSalesAsync(DateTime from, DateTime to, int? categoryId);
    Task<ProfitLossReportDto> GetProfitLossReportAsync(DateTime from, DateTime to);
    Task<List<LowStockItemRow>> GetLowStockReportAsync(decimal threshold);
}

