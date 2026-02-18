using POS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace POS.UI.Core.Services;

public class CustomerPaymentApiService : BaseApiService
{
    public CustomerPaymentApiService(HttpClient http) : base(http) { }

    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<List<CustomerBalanceDto>> GetOutstandingCustomersAsync()
    {
        var response = await _http.GetAsync("api/customer-payments/outstanding");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<CustomerBalanceDto>>(json, _jsonOptions) ?? new();
    }

    public async Task<CustomerLedgerDto?> GetLedgerAsync(Guid customerId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var url = $"api/customer-payments/{customerId}/ledger";
        if (fromDate.HasValue && toDate.HasValue)
            url += $"?fromDate={fromDate.Value:yyyy-MM-dd}&toDate={toDate.Value:yyyy-MM-dd}";

        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CustomerLedgerDto>(json, _jsonOptions);
    }

    public async Task<CustomerTransactionDto> PayDueAsync(CustomerPaymentRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/customer-payments/pay", dto);
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to process payment: {json}");
        return JsonSerializer.Deserialize<CustomerTransactionDto>(json, _jsonOptions)!;
    }
}
