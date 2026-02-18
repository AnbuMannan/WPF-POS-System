using System.Net.Http;
using System.Net.Http.Json;
using POS.Shared.Models;
using Serilog;

namespace POS.UI.Core.Services;

public class ImportApiService
{
    private readonly HttpClient _http;
    private readonly ILogger _logger;

    public ImportApiService(HttpClient httpClient)
    {
        _http = httpClient;
        _logger = Log.ForContext<ImportApiService>();
    }

    public async Task<ImportResultDto?> UploadProductsAsync(MultipartFormDataContent content)
    {
        try
        {
            var response = await _http.PostAsync("api/import/products", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ImportResultDto>();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error uploading products");
            throw;
        }
    }

    public async Task<byte[]> DownloadTemplateAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/import/template");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error downloading import template");
            throw;
        }
    }
}
