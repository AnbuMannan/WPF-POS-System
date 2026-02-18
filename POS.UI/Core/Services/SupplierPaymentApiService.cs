using POS.UI.Core.Exceptions;
using POS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;

namespace POS.UI.Core.Services;

public class SupplierPaymentApiService : BaseApiService
{
    public SupplierPaymentApiService(HttpClient http) : base(http) { }

    #region Payments

    public async Task<List<SupplierPaymentDto>> GetAllAsync(bool includeInactive = false)
    {
        try
        {
            var url = includeInactive ? "api/supplierpayments?includeInactive=true" : "api/supplierpayments";
            var json = await TryGetJsonAsync(url);
            if (string.IsNullOrWhiteSpace(json))
                return new List<SupplierPaymentDto>();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<List<SupplierPaymentDto>>(json, options);
            return result ?? new List<SupplierPaymentDto>();
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to fetch supplier payments.", ex);
        }
    }

    public async Task<SupplierPaymentDto?> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Payment ID must be valid.", nameof(id));
        try
        {
            var result = await _http.GetFromJsonAsync<SupplierPaymentDto>($"api/supplierpayments/{id}");
            return result;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to fetch supplier payment {id}.", ex);
        }
    }

    public async Task<List<SupplierPaymentDto>> GetBySupplierAsync(Guid supplierId)
    {
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier ID must be valid.", nameof(supplierId));
        try
        {
            var url = $"api/supplierpayments/supplier/{supplierId}";
            var result = await _http.GetFromJsonAsync<List<SupplierPaymentDto>>(url);
            return result ?? new List<SupplierPaymentDto>();
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to fetch payments for supplier {supplierId}.", ex);
        }
    }

    public async Task<SupplierPaymentDto> CreateAsync(CreateSupplierPaymentDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));
        if (dto.SupplierId == Guid.Empty)
            throw new ArgumentException("Supplier ID must be valid.", nameof(dto));
        if (dto.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(dto));

        _logger.Information("Creating supplier payment for Supplier: {SupplierId}, Amount: {Amount}", dto.SupplierId, dto.Amount);
        var response = await _http.PostAsJsonAsync("api/supplierpayments", dto);
        await EnsureSuccessAsync(response, "CreateSupplierPayment");
        
        var result = await response.Content.ReadFromJsonAsync<SupplierPaymentDto>();
        _logger.Information("Supplier payment created successfully: {PaymentNo}", result?.PaymentNo);
        return result ?? throw new HttpRequestException("Failed to deserialize payment response.");
    }

    public async Task<SupplierPaymentDto> UpdateAsync(Guid id, CreateSupplierPaymentDto dto)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Payment ID must be valid.", nameof(id));
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        _logger.Information("Updating supplier payment: {PaymentId}", id);
        var response = await _http.PutAsJsonAsync($"api/supplierpayments/{id}", dto);
        await EnsureSuccessAsync(response, "UpdateSupplierPayment");
        
        var result = await response.Content.ReadFromJsonAsync<SupplierPaymentDto>();
        _logger.Information("Supplier payment updated successfully: {PaymentId}", id);
        return result ?? throw new HttpRequestException("Failed to deserialize payment response.");
    }

    public async Task DisableAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Payment ID must be valid.", nameof(id));
        var response = await _http.DeleteAsync($"api/supplierpayments/{id}");
        await EnsureSuccessAsync(response, "DisableSupplierPayment");
    }

    #endregion

    #region Ledger / Transactions

    public async Task<List<SupplierTransactionDto>> GetLedgerAsync(Guid supplierId)
    {
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier ID must be valid.", nameof(supplierId));
        try
        {
            var url = $"api/supplierpayments/ledger/{supplierId}";
            var result = await _http.GetFromJsonAsync<List<SupplierTransactionDto>>(url);
            return result ?? new List<SupplierTransactionDto>();
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to fetch ledger for supplier {supplierId}.", ex);
        }
    }

    public async Task<decimal> GetBalanceAsync(Guid supplierId)
    {
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier ID must be valid.", nameof(supplierId));
        try
        {
            var url = $"api/supplierpayments/balance/{supplierId}";
            var result = await _http.GetFromJsonAsync<decimal>(url);
            return result;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to fetch balance for supplier {supplierId}.", ex);
        }
    }

    public async Task<List<SupplierBalanceDto>> GetAllBalancesAsync()
    {
        try
        {
            var url = "api/supplierpayments/balances";
            var json = await TryGetJsonAsync(url);
            if (string.IsNullOrWhiteSpace(json))
                return new List<SupplierBalanceDto>();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<List<SupplierBalanceDto>>(json, options);
            return result ?? new List<SupplierBalanceDto>();
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to fetch supplier balances.", ex);
        }
    }

    #endregion
}
