using Dapper;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;
using System.Data;

namespace POS.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IDbConnection _db;

    public ProductRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<Product> GetByIdAsync(Guid id)
        => await _db.QueryFirstOrDefaultAsync<Product>(
            "SELECT * FROM Products WHERE ProductId=@id AND IsActive=1", new { id });

    public async Task<Product> GetByBarcodeAsync(string barcode)
        => await _db.QueryFirstOrDefaultAsync<Product>(
            "SELECT * FROM Products WHERE Barcode=@barcode AND IsActive=1", new { barcode });

    public async Task<List<Product>> SearchAsync(string keyword)
    => (await _db.QueryAsync<Product>(@"
        SELECT * FROM Products 
        WHERE IsActive = 1
        AND (
            Name LIKE @k 
            OR SKU LIKE @k 
            OR Barcode LIKE @k
        )
        ORDER BY UpdatedAt DESC",
        new { k = "%" + keyword + "%" })).ToList();


    public async Task AddAsync(Product product)
    {
        await _db.ExecuteAsync(@"
        INSERT INTO Products
            (ProductId,Name,SKU,Barcode,Description,CategoryId,BrandId,Unit,
             CostPrice,SellingPrice,MRP,TaxProfileId,HSNCode,
             IsWeighable,IsManufactured,IsTaxInclusive,IsActive,CreatedAt,UpdatedAt)
            VALUES
            (@ProductId,@Name,@SKU,@Barcode,@Description,@CategoryId,@BrandId,@Unit,
             @CostPrice,@SellingPrice,@MRP,@TaxProfileId,@HSNCode,
             @IsWeighable,@IsManufactured,@IsTaxInclusive,@IsActive,@CreatedAt,@UpdatedAt)
            ", product);
                }

    public async Task UpdateAsync(Product product)
    {
        await _db.ExecuteAsync(@"
        UPDATE Products SET
        Name=@Name,SKU=@SKU,Barcode=@Barcode,Description=@Description,
        CategoryId=@CategoryId,BrandId=@BrandId,Unit=@Unit,
        CostPrice=@CostPrice,SellingPrice=@SellingPrice,MRP=@MRP,
        TaxProfileId=@TaxProfileId,
        IsWeighable=@IsWeighable,IsManufactured=@IsManufactured,
        IsTaxInclusive = @IsTaxInclusive, IsActive= @IsActive,
        UpdatedAt=@UpdatedAt
        WHERE ProductId=@ProductId", product);
    }

    public async Task DisableAsync(Guid id)
        => await _db.ExecuteAsync(
            "UPDATE Products SET IsActive=0 WHERE ProductId=@id", new { id });

    public async Task<List<Product>> GetAllAsync(bool showInactive)
    {
        var sql = showInactive
            ? "SELECT * FROM Products ORDER BY UpdatedAt DESC"
            : "SELECT * FROM Products WHERE IsActive=1 ORDER BY UpdatedAt DESC";

        return (await _db.QueryAsync<Product>(sql)).ToList();
    }

    public async Task<bool> SKUExistsAsync(string sku, Guid? excludeId = null)
    {
        var sql = @"SELECT COUNT(1) FROM Products 
                WHERE SKU = @sku";

        if (excludeId != null)
            sql += " AND ProductId <> @excludeId";

        var count = await _db.ExecuteScalarAsync<int>(sql, new { sku, excludeId });
        return count > 0;
    }

    public async Task<bool> BarcodeExistsAsync(string barcode, Guid? excludeId = null)
    {
        var sql = @"SELECT COUNT(1) FROM Products 
                WHERE Barcode = @barcode";

        if (excludeId != null)
            sql += " AND ProductId <> @excludeId";

        var count = await _db.ExecuteScalarAsync<int>(sql, new { barcode, excludeId });
        return count > 0;
    }

}
