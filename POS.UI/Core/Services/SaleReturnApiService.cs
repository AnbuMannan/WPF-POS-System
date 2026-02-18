using POS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace POS.UI.Core.Services;

public class SaleReturnApiService : BaseApiService
{
    public SaleReturnApiService(HttpClient http) : base(http) { }

    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<List<SaleReturnDto>> GetAllAsync()
    {
        var response = await _http.GetAsync("api/sale-returns");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<SaleReturnDto>>(json, _jsonOptions) ?? new();
    }

    public async Task<SaleReturnDto?> GetByIdAsync(int id)
    {
        try
        {
            return await _http.GetFromJsonAsync<SaleReturnDto>($"api/sale-returns/{id}", _jsonOptions);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<SaleInvoiceForReturnDto?> LookupInvoiceAsync(string billNumber)
    {
        try
        {
            return await _http.GetFromJsonAsync<SaleInvoiceForReturnDto>($"api/sale-returns/lookup-invoice/{Uri.EscapeDataString(billNumber)}", _jsonOptions);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<SaleReturnDto> CreateAsync(CreateSaleReturnDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/sale-returns", dto);
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to create sale return: {json}");
        return JsonSerializer.Deserialize<SaleReturnDto>(json, _jsonOptions)!;
    }

    public async Task<SaleReturnDto> ProcessAsync(int id)
    {
        var response = await _http.PostAsync($"api/sale-returns/{id}/process", null);
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to process sale return: {json}");
        return JsonSerializer.Deserialize<SaleReturnDto>(json, _jsonOptions)!;
    }
}
