using System.Net.Http;
using System.Net.Http.Json;
using POS.Shared.Models;

namespace POS.UI.Core.Services
{
    public class BillingApiService : BaseApiService
    {
        public BillingApiService(HttpClient httpClient) : base(httpClient) { }

        public async Task<ReceiptDto?> CreateSaleAsync(CreateSaleDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/billing/sales", dto);
            await EnsureSuccessAsync(response, "CreateSale");
            return await response.Content.ReadFromJsonAsync<ReceiptDto>();
        }

        public async Task<ReceiptDto?> GetReceiptBySaleIdAsync(int saleId)
        {
            return await _http.GetFromJsonAsync<ReceiptDto>($"api/billing/sales/{saleId}/receipt");
        }

        public async Task<string> GenerateBillNumberAsync()
        {
            var result = await _http.GetFromJsonAsync<BillNumberResponse>("api/billing/sales/generate-bill-number");
            return result?.BillNumber ?? $"INV{DateTime.Now:yyyyMMddHHmmss}";
        }

        public async Task<HeldBillDto?> HoldBillAsync(string reason, string cartData, decimal totalAmount)
        {
            var dto = new { Reason = reason, CartData = cartData, TotalAmount = totalAmount };
            var response = await _http.PostAsJsonAsync("api/billing/held-bills", dto);
            await EnsureSuccessAsync(response, "HoldBill");
            return await response.Content.ReadFromJsonAsync<HeldBillDto>();
        }

        public async Task<List<HeldBillDto>?> GetHeldBillsAsync()
        {
            return await _http.GetFromJsonAsync<List<HeldBillDto>>("api/billing/held-bills");
        }

        public async Task<bool> DeleteHeldBillAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/billing/held-bills/{id}");
            await EnsureSuccessAsync(response, "DeleteHeldBill");
            return true;
        }

        public async Task<DraftBillDto?> SaveDraftAsync(string? draftName, string? billNumber, string cartData, decimal totalAmount)
        {
            var dto = new { DraftName = draftName, BillNumber = billNumber, CartData = cartData, TotalAmount = totalAmount };
            var response = await _http.PostAsJsonAsync("api/billing/draft-bills", dto);
            await EnsureSuccessAsync(response, "SaveDraft");
            return await response.Content.ReadFromJsonAsync<DraftBillDto>();
        }

        public async Task<List<DraftBillDto>?> GetDraftBillsAsync()
        {
            return await _http.GetFromJsonAsync<List<DraftBillDto>>("api/billing/draft-bills");
        }

        public async Task<bool> DeleteDraftBillAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/billing/draft-bills/{id}");
            await EnsureSuccessAsync(response, "DeleteDraftBill");
            return true;
        }
    }

    public class BillNumberResponse
    {
        public string BillNumber { get; set; } = string.Empty;
    }
}
