using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Services
{
    public class CashTransactionService : ICashTransactionService
    {
        private readonly ICashTransactionRepository _repo;

        public CashTransactionService(ICashTransactionRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<CashTransactionDto>> GetAllAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var entities = await _repo.GetAllAsync(fromDate, toDate);
            return entities.Select(MapToDto).ToList();
        }

        public async Task<List<CashTransactionDto>> GetTodayAsync()
        {
            var entities = await _repo.GetTodayAsync();
            return entities.Select(MapToDto).ToList();
        }

        public async Task<CashTransactionDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return entity == null ? null : MapToDto(entity);
        }

        public async Task<CashTransactionDto> AddAsync(CreateCashTransactionDto dto, int userId, string userName)
        {
            if (dto.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero");

            if (string.IsNullOrWhiteSpace(dto.Type) || (dto.Type != "CashIn" && dto.Type != "CashOut"))
                throw new ArgumentException("Type must be 'CashIn' or 'CashOut'");

            var entity = new CashTransaction
            {
                TransactionDate = DateTime.Now,
                Type = dto.Type,
                Amount = dto.Amount,
                Description = dto.Description,
                Category = dto.Category,
                UserId = userId,
                UserName = userName,
                Remarks = dto.Remarks,
                ReferenceNo = GenerateReferenceNo(dto.Type)
            };

            await _repo.AddAsync(entity);
            return MapToDto(entity);
        }

        public async Task<CashSummaryDto> GetSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var totalIn = await _repo.GetTotalCashInAsync(fromDate, toDate);
            var totalOut = await _repo.GetTotalCashOutAsync(fromDate, toDate);
            var transactions = await _repo.GetAllAsync(fromDate, toDate);

            return new CashSummaryDto
            {
                TotalCashIn = totalIn,
                TotalCashOut = totalOut,
                CurrentBalance = totalIn - totalOut,
                TransactionCount = transactions.Count,
                LastTransactionDate = transactions.FirstOrDefault()?.TransactionDate
            };
        }

        public async Task<CashSummaryDto> GetTodaySummaryAsync()
        {
            var today = DateTime.Today;
            return await GetSummaryAsync(today, today);
        }

        private static string GenerateReferenceNo(string type)
        {
            var prefix = type == "CashIn" ? "CI" : "CO";
            return $"{prefix}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmmss}";
        }

        private static CashTransactionDto MapToDto(CashTransaction entity)
        {
            return new CashTransactionDto
            {
                Id = entity.Id,
                TransactionDate = entity.TransactionDate,
                Type = entity.Type,
                Amount = entity.Amount,
                Description = entity.Description,
                ReferenceNo = entity.ReferenceNo,
                Category = entity.Category,
                UserId = entity.UserId,
                UserName = entity.UserName,
                Remarks = entity.Remarks,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
