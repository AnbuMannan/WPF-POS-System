using POS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace POS.UI.Core.Services;

public class QuotationApiService : BaseApiService
{
    public QuotationApiService(HttpClient http) : base(http) { }

    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<List<QuotationDto>> GetAllAsync(bool includeInactive = false)
    {
        var url = includeInactive ? "api/quotations?includeInactive=true" : "api/quotations";
        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<QuotationDto>>(json, _jsonOptions) ?? new();
    }

    public async Task<QuotationDto?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _http.GetFromJsonAsync<QuotationDto>($"api/quotations/{id}", _jsonOptions);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<QuotationDto> CreateAsync(CreateQuotationDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/quotations", dto);
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to create quotation: {json}");
        return JsonSerializer.Deserialize<QuotationDto>(json, _jsonOptions)!;
    }

    public async Task<QuotationDto> UpdateAsync(Guid id, CreateQuotationDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/quotations/{id}", dto);
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to update quotation: {json}");
        return JsonSerializer.Deserialize<QuotationDto>(json, _jsonOptions)!;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/quotations/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ConvertToSaleAsync(Guid id)
    {
        var response = await _http.PostAsync($"api/quotations/{id}/convert-to-sale", null);
        return response.IsSuccessStatusCode;
    }
}
