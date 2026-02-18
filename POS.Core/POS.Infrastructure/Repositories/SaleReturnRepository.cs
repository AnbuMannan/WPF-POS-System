using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class SaleReturnRepository : ISaleReturnRepository
{
    private readonly PosDbContext _context;

    public SaleReturnRepository(PosDbContext context)
    {
        _context = context;
    }

    public async Task<List<SaleReturn>> GetAllAsync()
    {
        return await _context.Returns
            .Include(r => r.OriginalSale)
            .Include(r => r.Customer)
            .Include(r => r.ReturnItems)
                .ThenInclude(ri => ri.SaleItem)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<SaleReturn?> GetByIdAsync(int id)
    {
        return await _context.Returns
            .Include(r => r.OriginalSale)
            .Include(r => r.Customer)
            .Include(r => r.ReturnItems)
                .ThenInclude(ri => ri.SaleItem)
            .FirstOrDefaultAsync(r => r.ReturnId == id);
    }

    public async Task<List<SaleReturn>> GetBySaleIdAsync(long saleId)
    {
        return await _context.Returns
            .Include(r => r.ReturnItems)
            .Where(r => r.OriginalSaleId == saleId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Sale?> GetSaleWithItemsAsync(long saleId)
    {
        return await _context.Sales
            .Include(s => s.SaleItems)
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.SaleId == saleId);
    }

    public async Task<Sale?> GetSaleByBillNumberAsync(string billNumber)
    {
        return await _context.Sales
            .Include(s => s.SaleItems)
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.BillNumber == billNumber || s.InvoiceNumber == billNumber);
    }

    public async Task<SaleReturn> CreateAsync(SaleReturn saleReturn)
    {
        _context.Returns.Add(saleReturn);
        await _context.SaveChangesAsync();

        // Re-fetch with includes
        return (await GetByIdAsync(saleReturn.ReturnId))!;
    }

    public async Task ProcessReturnWithInventoryAsync(int returnId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var saleReturn = await _context.Returns
                .Include(r => r.ReturnItems)
                .FirstOrDefaultAsync(r => r.ReturnId == returnId);

            if (saleReturn == null) throw new Exception("Sale return not found.");

            foreach (var item in saleReturn.ReturnItems)
            {
                if (item.IsRestockable)
                {
                    // Update StockSummary via raw SQL (Dapper style in EF)
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE StockSummary SET AvailableStock = AvailableStock + {0}, LastUpdated = NOW() WHERE ProductId = {1}",
                        item.QuantityReturned, item.ProductId);

                    // Insert stock ledger entry
                    var ledgerEntry = new StockLedgerEntry
                    {
                        StockEntryId = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        Quantity = item.QuantityReturned,
                        EntryType = "RETURN",
                        ReferenceType = "SALES_RETURN",
                        ReferenceId = null,
                        EntryDate = DateTime.Now,
                        Remarks = $"Sales Return: {saleReturn.ReturnNumber} - {item.ProductName}"
                    };
                    _context.StockLedgerEntries.Add(ledgerEntry);
                }

                // Mark sale item as returned if fully returned
                var saleItem = await _context.SaleItems.FindAsync(item.SaleItemId);
                if (saleItem != null)
                {
                    var totalReturned = await _context.ReturnItems
                        .Where(ri => ri.SaleItemId == item.SaleItemId)
                        .SumAsync(ri => ri.QuantityReturned);

                    if (totalReturned >= saleItem.Quantity)
                        saleItem.IsReturned = true;
                }
            }

            saleReturn.IsProcessed = true;
            saleReturn.Status = "Processed";

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<string> GenerateReturnNumberAsync()
    {
        var today = DateTime.Now;
        var prefix = $"SR-{today:yyyyMMdd}";
        var count = await _context.Returns
            .CountAsync(r => r.ReturnNumber.StartsWith(prefix));
        return $"{prefix}-{(count + 1):D3}";
    }

    public async Task<decimal> GetAlreadyReturnedQuantityAsync(long saleItemId)
    {
        return await _context.ReturnItems
            .Where(ri => ri.SaleItemId == saleItemId)
            .SumAsync(ri => ri.QuantityReturned);
    }
}
