using System.Net.Http;
using System.Net.Http.Json;
using POS.Shared.Models;

namespace POS.UI.Core.Services
{
    public class AuditLogApiService : BaseApiService
    {
        public AuditLogApiService(HttpClient httpClient) : base(httpClient) { }

        public async Task<List<AuditLogDto>?> GetLogsAsync(DateTime? from = null, DateTime? to = null)
        {
            var query = $"api/auditlogs?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
            return await _http.GetFromJsonAsync<List<AuditLogDto>>(query);
        }

        public async Task<bool> CreateLogAsync(string action, string entity, string details)
        {
            var dto = new { Action = action, Entity = entity, Details = details };
            var response = await _http.PostAsJsonAsync("api/auditlogs", dto);
            await EnsureSuccessAsync(response, "CreateAuditLog");
            return true;
        }
    }

    public class AuditLogDto
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Username { get; set; }
    }
}
