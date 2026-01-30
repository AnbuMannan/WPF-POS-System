using POS.UI.Core.Exceptions;
using POS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;

namespace POS.UI.Core.Services;

public class TaxProfileApiService : BaseApiService
{
    public TaxProfileApiService(HttpClient http) : base(http) { }

    public async Task<List<TaxProfileDto>> GetAllAsync(bool includeInactive = false)
    {
        try
        {
            var url = includeInactive ? "api/taxprofiles?includeInactive=true" : "api/taxprofiles";
            var json = await TryGetJsonAsync(url, url);
            if (string.IsNullOrWhiteSpace(json))
                return new List<TaxProfileDto>();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<List<TaxProfileDto>>(json, options);
            return result ?? new List<TaxProfileDto>();
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to fetch tax profiles.", ex);
        }
    }

    public async Task<TaxProfileDto> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Tax profile ID must be greater than 0.", nameof(id));
        try
        {
            var result = await _http.GetFromJsonAsync<TaxProfileDto>($"api/taxprofiles/{id}");
            return result ?? throw new HttpRequestException($"Tax profile with ID {id} not found.");
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to fetch tax profile {id}.", ex);
        }
    }

    public async Task AddAsync(TaxProfileDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Tax profile name cannot be empty.", nameof(dto));
        _logger.Information("Creating new tax profile: {Name}", dto.Name);
        var response = await _http.PostAsJsonAsync("api/taxprofiles", dto);
        await EnsureSuccessAsync(response, "CreateTaxProfile");
        _logger.Information("Tax profile created successfully: {Name}", dto.Name);
    }

    public async Task UpdateAsync(TaxProfileDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));
        if (dto.TaxProfileId <= 0)
            throw new ArgumentException("Tax profile ID must be greater than 0.", nameof(dto));
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Tax profile name cannot be empty.", nameof(dto));
        _logger.Information("Updating tax profile: {Id} - {Name}", dto.TaxProfileId, dto.Name);
        var response = await _http.PutAsJsonAsync("api/taxprofiles", dto);
        await EnsureSuccessAsync(response, "UpdateTaxProfile");
        _logger.Information("Tax profile updated successfully: {Id}", dto.TaxProfileId);
    }

    public async Task DisableAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Tax profile ID must be greater than 0.", nameof(id));
        var response = await _http.DeleteAsync($"api/taxprofiles/{id}");
        await EnsureSuccessAsync(response);
    }
}
