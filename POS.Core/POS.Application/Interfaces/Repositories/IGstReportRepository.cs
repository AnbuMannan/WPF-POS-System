using POS.Application.DTOs.Reports;

namespace POS.Application.Interfaces.Repositories;

public interface IGstReportRepository
{
    Task<GstSummaryDto> GetGstSummaryAsync(DateTime from, DateTime to);
    Task<List<GstHsnSummaryDto>> GetHsnSummaryAsync(DateTime from, DateTime to);
    Task<List<GstDailyCollectionDto>> GetDailyCollectionAsync(DateTime from, DateTime to);
}
