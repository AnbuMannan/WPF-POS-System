using POS.Shared.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace POS.UI.Core.Services;

public class ExpenseApiService : BaseApiService
{
    public ExpenseApiService(HttpClient http) : base(http) { }

    public async Task<List<ExpenseDto>> GetExpensesAsync(DateTime? date = null)
    {
        var url = "api/expenses";
        if (date.HasValue)
            url += $"?date={date.Value:yyyy-MM-dd}";
        var result = await _http.GetFromJsonAsync<List<ExpenseDto>>(url);
        return result ?? new List<ExpenseDto>();
    }

    public async Task<ExpenseDto?> CreateExpenseAsync(ExpenseDto dto)
    {
        // A1-Grade: Ensure StoreCode is attached from AppState before sending to API
        dto.StoreCode = POS.UI.Core.AppState.CurrentStoreCode;
        var response = await _http.PostAsJsonAsync("api/expenses", dto);
        await EnsureSuccessAsync(response, "CreateExpense");
        var saved = await response.Content.ReadFromJsonAsync<ExpenseDto>();
        return saved;
    }

    public async Task<bool> DeleteExpenseAsync(Guid id)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/expenses/{id}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
