using Dapper;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using System.Data;

namespace POS.Infrastructure.Repositories;

public class BrandRepository : IBrandRepository
{
    private readonly IDbConnection _db;

    public BrandRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<List<Brand>> GetAllAsync()
        => (await _db.QueryAsync<Brand>(
            "SELECT BrandId AS Id,Name FROM Brands WHERE IsActive=1 ORDER BY Name")).ToList();

    public async Task<Brand> GetByIdAsync(Guid id)
        => await _db.QueryFirstOrDefaultAsync<Brand>(
            "SELECT * FROM Brands WHERE BrandId=@id", new { id });

    public async Task AddAsync(Brand brand)
    {
        await _db.ExecuteAsync(
            "INSERT INTO Brands(BrandId,Name,IsActive) VALUES(@BrandId,@Name,@IsActive)", brand);
    }

    public async Task UpdateAsync(Brand brand)
    {
        await _db.ExecuteAsync(
            "UPDATE Brands SET Name=@Name WHERE BrandId=@BrandId", brand);
    }

    public async Task DisableAsync(Guid id)
        => await _db.ExecuteAsync(
            "UPDATE Brands SET IsActive=0 WHERE BrandId=@id", new { id });
}
