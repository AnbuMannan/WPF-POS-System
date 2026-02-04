namespace POS.Application.Interfaces.Services;

public interface IAuditLogService
{
    Task LogAsync(string userId, string action, string entityType, string? entityId, string? oldValue, string? newValue, CancellationToken cancellationToken = default);
}
