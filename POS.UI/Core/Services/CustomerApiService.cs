using POS.UI.Core.Exceptions;
using POS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace POS.UI.Core.Services;

public class CustomerApiService : BaseApiService
{
    public CustomerApiService(HttpClient http) : base(http) { }

    public async Task<List<CustomerDto>> GetAllAsync(bool includeInactive = false)
    {
        try
        {
            var url = includeInactive ? "api/customers?includeInactive=true" : "api/customers";
            var json = await _http.GetStringAsync(url);
            if (string.IsNullOrWhiteSpace(json))
                return new List<CustomerDto>();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<List<CustomerDto>>(json, options);
            return result ?? new List<CustomerDto>();
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to fetch customers.", ex);
        }
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<CustomerDto>($"api/customers/{id}");
            return result;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to fetch customer {id}.", ex);
        }
    }

    public async Task AddAsync(CustomerDto customer)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));
        if (string.IsNullOrWhiteSpace(customer.Name))
            throw new ArgumentException("Customer name cannot be empty.", nameof(customer));
        _logger.Information("Creating new customer: {Name}", customer.Name);
        var response = await _http.PostAsJsonAsync("api/customers", customer);
        await EnsureSuccessAsync(response, "CreateCustomer");
        _logger.Information("Customer created successfully: {Name}", customer.Name);
    }

    public async Task UpdateAsync(CustomerDto customer)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));
        if (customer.Id == Guid.Empty)
            throw new ArgumentException("Customer Id must be set for update.", nameof(customer));
        if (string.IsNullOrWhiteSpace(customer.Name))
            throw new ArgumentException("Customer name cannot be empty.", nameof(customer));
        _logger.Information("Updating customer: {Id} - {Name}", customer.Id, customer.Name);
        var response = await _http.PutAsJsonAsync("api/customers", customer);
        await EnsureSuccessAsync(response, "UpdateCustomer");
        _logger.Information("Customer updated successfully: {Id}", customer.Id);
    }

    public async Task DisableAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/customers/{id}");
        await EnsureSuccessAsync(response);
    }

    public async Task<bool> CheckPhoneExistsAsync(string? phone, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;
        try
        {
            var url = $"api/customers/exists/phone?phone={Uri.EscapeDataString(phone)}";
            if (excludeId.HasValue && excludeId.Value != Guid.Empty)
                url += $"&excludeId={excludeId}";
            var result = await _http.GetFromJsonAsync<bool>(url);
            return result;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to check phone availability.", ex);
        }
    }
}
