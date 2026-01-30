using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly PosDbContext _db;

    public ProductRepository(PosDbContext db) => _db = db;

    public async Task<Product> GetByIdAsync(long id)
        => await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive);

    public async Task<Product> GetByBarcodeAsync(string barcode)
        => await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.Barcode == barcode && p.IsActive);

    public async Task<List<Product>> SearchAsync(string keyword)
        => await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => EF.Functions.Like(p.Name, "%" + keyword + "%")
                     || EF.Functions.Like(p.SKU, "%" + keyword + "%")
                     || (p.Barcode != null && EF.Functions.Like(p.Barcode, "%" + keyword + "%")))
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();

    public async Task AddAsync(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        var existing = await _db.Products
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == product.ProductId);
        if (existing == null) return;

        _db.Entry(product).Property(p => p.RowVersion).OriginalValue = existing.RowVersion;
        _db.Entry(product).Property(p => p.CreatedAt).IsModified = false;
        _db.Products.Update(product);
        await _db.SaveChangesAsync();
    }

    public async Task DisableAsync(long id)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == id);
        if (entity == null) return;
        entity.IsActive = false;
        await _db.SaveChangesAsync();
    }

    public async Task<List<Product>> GetAllAsync(bool showInactive)
    {
        IQueryable<Product> query = _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand);
        if (showInactive) query = query.IgnoreQueryFilters();
        return await query.OrderByDescending(p => p.UpdatedAt).ToListAsync();
    }

    public async Task<bool> SKUExistsAsync(string sku, long? excludeId = null)
    {
        var query = _db.Products.AsNoTracking().Where(p => p.SKU == sku);
        if (excludeId != null) query = query.Where(p => p.ProductId != excludeId);
        return await query.AnyAsync();
    }

    public async Task<bool> BarcodeExistsAsync(string barcode, long? excludeId = null)
    {
        var query = _db.Products.AsNoTracking().Where(p => p.Barcode == barcode);
        if (excludeId != null) query = query.Where(p => p.ProductId != excludeId);
        return await query.AnyAsync();
    }
}
