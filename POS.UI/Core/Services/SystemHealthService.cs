using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace POS.UI.Core.Services
{
    public class ServiceHealthResult
    {
        public bool IsOnline { get; set; }
        public long LatencyMs { get; set; }
        public string? Error { get; set; }
    }

    public class SystemHealthService
    {
        private readonly IConfiguration _config;
        private readonly LocalSettingsService _local;

        public SystemHealthService(IConfiguration config, LocalSettingsService local)
        {
            _config = config;
            _local = local;
        }

        public async Task<ServiceHealthResult> CheckServiceAsync(string serviceName, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return new ServiceHealthResult { IsOnline = false, LatencyMs = 0, Error = "BaseUrl missing" };
            }

            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
            var uri = baseUrl.TrimEnd('/') + "/api/health/ping";
            var sw = Stopwatch.StartNew();
            try
            {
                var resp = await client.GetAsync(uri);
                sw.Stop();
                if (resp.IsSuccessStatusCode)
                {
                    return new ServiceHealthResult { IsOnline = true, LatencyMs = sw.ElapsedMilliseconds };
                }
                return new ServiceHealthResult
                {
                    IsOnline = false,
                    LatencyMs = sw.ElapsedMilliseconds,
                    Error = ((int)resp.StatusCode).ToString()
                };
            }
            catch (Exception ex)
            {
                sw.Stop();
                return new ServiceHealthResult { IsOnline = false, LatencyMs = sw.ElapsedMilliseconds, Error = ex.Message };
            }
        }
    }
}
