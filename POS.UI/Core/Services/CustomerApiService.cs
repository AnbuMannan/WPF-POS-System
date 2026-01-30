using POS.UI.Core.Exceptions;
using POS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;

namespace POS.UI.Core.Services;

public class CustomerApiService : BaseApiService
{
    public CustomerApiService(HttpClient http) : base(http) { }

    public async Task<List<CustomerDto>> GetAllAsync()
    {
        try
        {
            var json = await TryGetJsonAsync("api/customers/all", "api/customers");
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

    public async Task<CustomerDto> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Customer ID cannot be empty.", nameof(id));
        try
        {
            var result = await _http.GetFromJsonAsync<CustomerDto>($"api/customers/{Uri.EscapeDataString(id)}");
            return result ?? throw new HttpRequestException($"Customer with ID {id} not found.");
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
        if (string.IsNullOrWhiteSpace(customer.FirstName))
            throw new ArgumentException("First name cannot be empty.", nameof(customer));
        _logger.Information("Creating new customer: {FirstName} {LastName}", customer.FirstName, customer.LastName);
        var response = await _http.PostAsJsonAsync("api/customers", customer);
        await EnsureSuccessAsync(response, "CreateCustomer");
        _logger.Information("Customer created successfully: {CustomerId} - {FullName}", customer.CustomerId, customer.FullName);
    }

    public async Task UpdateAsync(CustomerDto customer)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));
        if (string.IsNullOrWhiteSpace(customer.CustomerId))
            throw new ArgumentException("Customer ID cannot be empty.", nameof(customer));
        if (string.IsNullOrWhiteSpace(customer.FirstName))
            throw new ArgumentException("First name cannot be empty.", nameof(customer));
        _logger.Information("Updating customer: {CustomerId} - {FullName}", customer.CustomerId, customer.FullName);
        var response = await _http.PutAsJsonAsync("api/customers", customer);
        await EnsureSuccessAsync(response, "UpdateCustomer");
        _logger.Information("Customer updated successfully: {CustomerId}", customer.CustomerId);
    }

    public async Task DisableAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Customer ID cannot be empty.", nameof(id));
        var response = await _http.DeleteAsync($"api/customers/{Uri.EscapeDataString(id)}");
        await EnsureSuccessAsync(response);
    }

    public async Task<bool> CheckPhoneExistsAsync(string phone, string? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;
        try
        {
            var url = $"api/customers/exists/phone?phone={Uri.EscapeDataString(phone)}";
            if (!string.IsNullOrWhiteSpace(excludeId))
                url += $"&excludeId={Uri.EscapeDataString(excludeId)}";
            var result = await _http.GetFromJsonAsync<bool>(url);
            return result;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to check phone availability.", ex);
        }
    }
}
