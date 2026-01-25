using POS.Application.DTOs.Reports;
using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;

namespace POS.Application.Services;

public class GstReportService : IGstReportService
{
    private readonly IGstReportRepository _repo;

    public GstReportService(IGstReportRepository repo)
    {
        _repo = repo;
    }

    public async Task<GstSummaryDto> GetSummaryAsync(DateTime from, DateTime to)
        => await _repo.GetGstSummaryAsync(from, to);

    public async Task<List<GstHsnSummaryDto>> GetHsnSummaryAsync(DateTime from, DateTime to)
        => await _repo.GetHsnSummaryAsync(from, to);

    public async Task<List<GstDailyCollectionDto>> GetDailyCollectionAsync(DateTime from, DateTime to)
        => await _repo.GetDailyCollectionAsync(from, to);
}
