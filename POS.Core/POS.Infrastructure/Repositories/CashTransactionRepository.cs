using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories
{
    public class CashTransactionRepository : ICashTransactionRepository
    {
        private readonly PosDbContext _db;

        public CashTransactionRepository(PosDbContext db)
        {
            _db = db;
        }

        public async Task<List<CashTransaction>> GetAllAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _db.CashTransactions.AsNoTracking().Where(x => x.IsActive);

            if (fromDate.HasValue)
                query = query.Where(x => x.TransactionDate >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(x => x.TransactionDate <= toDate.Value.Date.AddDays(1).AddSeconds(-1));

            return await query.OrderByDescending(x => x.TransactionDate)
                              .ThenByDescending(x => x.CreatedAt)
                              .ToListAsync();
        }

        public async Task<List<CashTransaction>> GetTodayAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _db.CashTransactions
                .AsNoTracking()
                .Where(x => x.IsActive && x.TransactionDate >= today && x.TransactionDate < tomorrow)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<CashTransaction?> GetByIdAsync(Guid id)
        {
            return await _db.CashTransactions.FindAsync(id);
        }

        public async Task AddAsync(CashTransaction entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.Now;
            entity.IsActive = true;
            await _db.CashTransactions.AddAsync(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(CashTransaction entity)
        {
            entity.UpdatedAt = DateTime.Now;
            _db.CashTransactions.Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<decimal> GetTotalCashInAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _db.CashTransactions.AsNoTracking()
                .Where(x => x.IsActive && x.Type == "CashIn");

            if (fromDate.HasValue)
                query = query.Where(x => x.TransactionDate >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(x => x.TransactionDate <= toDate.Value.Date.AddDays(1).AddSeconds(-1));

            return await query.SumAsync(x => x.Amount);
        }

        public async Task<decimal> GetTotalCashOutAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _db.CashTransactions.AsNoTracking()
                .Where(x => x.IsActive && x.Type == "CashOut");

            if (fromDate.HasValue)
                query = query.Where(x => x.TransactionDate >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(x => x.TransactionDate <= toDate.Value.Date.AddDays(1).AddSeconds(-1));

            return await query.SumAsync(x => x.Amount);
        }
    }
}
