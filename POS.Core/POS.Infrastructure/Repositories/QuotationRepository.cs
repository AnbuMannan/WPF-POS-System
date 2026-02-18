using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class QuotationRepository : IQuotationRepository
{
    private readonly PosDbContext _context;

    public QuotationRepository(PosDbContext context)
    {
        _context = context;
    }

    public async Task<List<Quotation>> GetAllAsync(bool includeInactive = false)
    {
        IQueryable<Quotation> query = _context.Quotations;

        if (includeInactive)
            query = query.IgnoreQueryFilters();

        return await query
            .Include(q => q.Customer)
            .Include(q => q.Items)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<Quotation?> GetByIdAsync(Guid id)
    {
        return await _context.Quotations
            .Include(q => q.Customer)
            .Include(q => q.Items)
                .ThenInclude(qi => qi.Product)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<Quotation> AddAsync(Quotation quotation)
    {
        _context.Quotations.Add(quotation);
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(quotation.Id))!;
    }

    public async Task<Quotation> UpdateAsync(Quotation quotation)
    {
        // Remove old items
        var existingItems = await _context.QuotationItems
            .Where(qi => qi.QuotationId == quotation.Id)
            .ToListAsync();
        _context.QuotationItems.RemoveRange(existingItems);

        _context.Quotations.Update(quotation);
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(quotation.Id))!;
    }

    public async Task DisableAsync(Guid id)
    {
        var quotation = await _context.Quotations.FindAsync(id);
        if (quotation != null)
        {
            quotation.IsActive = false;
            quotation.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateQuotationNumberAsync()
    {
        var today = DateTime.Now;
        var prefix = $"QT-{today:yyyyMMdd}";
        var count = await _context.Quotations
            .IgnoreQueryFilters()
            .CountAsync(q => q.QuotationNumber.StartsWith(prefix));
        return $"{prefix}-{(count + 1):D3}";
    }

    public async Task UpdateStatusAsync(Guid id, string status)
    {
        var quotation = await _context.Quotations.FindAsync(id);
        if (quotation != null)
        {
            quotation.Status = Enum.Parse<QuotationStatus>(status);
            quotation.UpdatedAt = DateTime.Now;
            if (status == "Converted")
            {
                quotation.ConvertedAt = DateTime.Now;
                quotation.ConvertedBy = "System";
            }
            await _context.SaveChangesAsync();
        }
    }
}
