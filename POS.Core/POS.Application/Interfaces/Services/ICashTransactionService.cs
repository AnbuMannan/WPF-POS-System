using POS.Shared.Models;

namespace POS.Application.Interfaces.Services
{
    public interface ICashTransactionService
    {
        Task<List<CashTransactionDto>> GetAllAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<CashTransactionDto>> GetTodayAsync();
        Task<CashTransactionDto?> GetByIdAsync(Guid id);
        Task<CashTransactionDto> AddAsync(CreateCashTransactionDto dto, int userId, string userName);
        Task<CashSummaryDto> GetSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<CashSummaryDto> GetTodaySummaryAsync();
    }
}
