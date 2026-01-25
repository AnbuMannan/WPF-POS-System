using POS.Application.DTOs.Reports;

namespace POS.Application.Interfaces.Services;

public interface IGstReportService
{
    Task<GstSummaryDto> GetSummaryAsync(DateTime from, DateTime to);
    Task<List<GstHsnSummaryDto>> GetHsnSummaryAsync(DateTime from, DateTime to);
    Task<List<GstDailyCollectionDto>> GetDailyCollectionAsync(DateTime from, DateTime to);
}
