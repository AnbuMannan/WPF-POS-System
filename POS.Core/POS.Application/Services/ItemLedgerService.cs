using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Services;

/// <summary>
/// Service for generating item ledger reports from StockLedgerEntry
/// </summary>
public class ItemLedgerService : IItemLedgerService
{
    private readonly IProductRepository _productRepo;
    private readonly IStockLedgerRepository _ledgerRepo;

    public ItemLedgerService(IProductRepository productRepo, IStockLedgerRepository ledgerRepo)
    {
        _productRepo = productRepo;
        _ledgerRepo = ledgerRepo;
    }

    public async Task<ItemLedgerResponseDto> GetLedgerAsync(long productId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null)
            throw new InvalidOperationException($"Product with ID {productId} not found.");

        var response = new ItemLedgerResponseDto
        {
            ProductId = productId,
            ProductName = product.Name,
            ProductSku = product.SKU
        };

        // Get all entries for this product ordered by date
        var allEntries = await _ledgerRepo.GetByProductIdAsync(productId);
        
        // Calculate opening balance (entries before fromDate)
        decimal openingBalance = 0;
        if (fromDate.HasValue)
        {
            var entriesBeforePeriod = allEntries
                .Where(e => e.EntryDate < fromDate.Value.Date)
                .ToList();
            
            foreach (var entry in entriesBeforePeriod)
            {
                if (entry.EntryType == "IN")
                    openingBalance += entry.Quantity;
                else if (entry.EntryType == "OUT")
                    openingBalance -= entry.Quantity;
            }
        }

        response.OpeningBalance = openingBalance;

        // Filter entries for the requested period
        var periodEntries = allEntries.AsEnumerable();
        
        if (fromDate.HasValue)
            periodEntries = periodEntries.Where(e => e.EntryDate >= fromDate.Value.Date);
        
        if (toDate.HasValue)
            periodEntries = periodEntries.Where(e => e.EntryDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));

        // Calculate running balance and map to DTOs
        decimal runningBalance = openingBalance;
        var ledgerEntries = new List<ItemLedgerDto>();

        foreach (var entry in periodEntries.OrderBy(e => e.EntryDate).ThenBy(e => e.StockEntryId))
        {
            decimal inQty = 0;
            decimal outQty = 0;

            if (entry.EntryType == "IN")
            {
                inQty = entry.Quantity;
                runningBalance += entry.Quantity;
            }
            else if (entry.EntryType == "OUT")
            {
                outQty = entry.Quantity;
                runningBalance -= entry.Quantity;
            }

            ledgerEntries.Add(new ItemLedgerDto
            {
                Id = entry.StockEntryId,
                ProductId = productId,
                ProductName = product.Name,
                ProductSku = product.SKU,
                EntryDate = entry.EntryDate,
                TransactionType = MapReferenceTypeToTransactionType(entry.ReferenceType),
                ReferenceNo = entry.Remarks ?? entry.ReferenceType,
                ReferenceId = entry.ReferenceId,
                InQty = inQty,
                OutQty = outQty,
                RunningBalance = runningBalance,
                Remarks = entry.Remarks,
                CreatedAt = entry.EntryDate
            });

            if (inQty > 0) response.TotalIn += inQty;
            if (outQty > 0) response.TotalOut += outQty;
        }

        response.Entries = ledgerEntries;
        response.ClosingBalance = runningBalance;

        return response;
    }

    public async Task<List<ItemLedgerDto>> GetEntriesAsync(long productId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var response = await GetLedgerAsync(productId, fromDate, toDate);
        return response.Entries;
    }

    private static string MapReferenceTypeToTransactionType(string referenceType)
    {
        return referenceType?.ToUpperInvariant() switch
        {
            "PURCHASE" or "PURCHASEENTRY" or "GRN" => "Purchase",
            "SALE" or "BILLING" => "Sale",
            "RETURN" or "SALESRETURN" => "Sales Return",
            "PURCHASERETURN" => "Purchase Return",
            "ADJUSTMENT" or "STOCKADJUSTMENT" => "Adjustment",
            "OPENING" or "OPENINGSTOCK" => "Opening Stock",
            "MANUAL" => "Manual",
            _ => referenceType ?? "Unknown"
        };
    }
}
