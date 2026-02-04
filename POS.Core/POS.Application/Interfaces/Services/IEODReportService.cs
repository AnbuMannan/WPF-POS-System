using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface IEODReportService
{
    Task<EODReportDto> GetEODReportAsync(DateTime date, CancellationToken cancellationToken = default);

    /// <summary>Marks all completed sales for the given date as locked (prevent editing).</summary>
    Task CloseDayReportAsync(DateTime date, string? lockedBy = null, CancellationToken cancellationToken = default);
}
