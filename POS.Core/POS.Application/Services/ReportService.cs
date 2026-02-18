using POS.Application.Interfaces.Services;
using POS.Shared.Models;
using POS.Application.Interfaces.Repositories;

namespace POS.Application.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _repo;

    public ReportService(IReportRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<SalesSummaryReportRow>> GetSalesReportAsync(DateTime from, DateTime to, Guid? customerId, string? status)
        => await _repo.GetSalesReportAsync(from, to, customerId, status);

    public async Task<List<ItemWiseSalesRow>> GetItemWiseSalesAsync(DateTime from, DateTime to, int? categoryId)
        => await _repo.GetItemWiseSalesAsync(from, to, categoryId);

    public async Task<ProfitLossReportDto> GetProfitLossReportAsync(DateTime from, DateTime to)
        => await _repo.GetProfitLossReportAsync(from, to);

    public async Task<List<LowStockItemRow>> GetLowStockReportAsync(decimal threshold)
        => await _repo.GetLowStockReportAsync(threshold);
}

