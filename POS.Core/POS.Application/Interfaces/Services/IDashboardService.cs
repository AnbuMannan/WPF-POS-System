using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(DateTime date, CancellationToken cancellationToken = default);
}

