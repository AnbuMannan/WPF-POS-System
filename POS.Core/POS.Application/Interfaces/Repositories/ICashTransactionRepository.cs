using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories
{
    public interface ICashTransactionRepository
    {
        Task<List<CashTransaction>> GetAllAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<CashTransaction>> GetTodayAsync();
        Task<CashTransaction?> GetByIdAsync(Guid id);
        Task AddAsync(CashTransaction entity);
        Task UpdateAsync(CashTransaction entity);
        Task<decimal> GetTotalCashInAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<decimal> GetTotalCashOutAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
}
