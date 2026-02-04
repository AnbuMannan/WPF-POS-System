using POS.Application.Interfaces.Services;

namespace POS.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    public Task LogAsync(string userId, string action, string entityType, string? entityId, string? oldValue, string? newValue, CancellationToken cancellationToken = default)
    {
        // Stub: no persistence until AuditLog entity matches DbContext
        return Task.CompletedTask;
    }
}
