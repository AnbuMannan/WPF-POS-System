using Dapper;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using System.Data;

namespace POS.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly IDbConnection _db;

    public CategoryRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<List<Category>> GetAllAsync()
        => (await _db.QueryAsync<Category>(
            "SELECT CategoryId AS Id,Name FROM Categories WHERE IsActive=1 ORDER BY DisplayOrder")).ToList();

    public async Task<Category> GetByIdAsync(Guid id)
        => await _db.QueryFirstOrDefaultAsync<Category>(
            "SELECT * FROM Categories WHERE CategoryId=@id", new { id });

    public async Task AddAsync(Category category)
    {
        await _db.ExecuteAsync(@"
        INSERT INTO Categories(CategoryId,Name,ParentCategoryId,DisplayOrder,IsActive)
        VALUES(@CategoryId,@Name,@ParentCategoryId,@DisplayOrder,@IsActive)", category);
    }

    public async Task UpdateAsync(Category category)
    {
        await _db.ExecuteAsync(@"
        UPDATE Categories SET
        Name=@Name,
        ParentCategoryId=@ParentCategoryId,
        DisplayOrder=@DisplayOrder
        WHERE CategoryId=@CategoryId", category);
    }

    public async Task DisableAsync(Guid id)
        => await _db.ExecuteAsync(
            "UPDATE Categories SET IsActive=0 WHERE CategoryId=@id", new { id });

    public async Task<bool> CheckNameExistsAsync(string name, Guid? parentCategoryId, Guid? excludeId)
    {
        var sql = @"
        SELECT COUNT(1)
        FROM Categories
        WHERE Name = @name
          AND IFNULL(ParentCategoryId, '00000000-0000-0000-0000-000000000000') =
              IFNULL(@parentCategoryId, '00000000-0000-0000-0000-000000000000')
          AND IsActive = 1
          AND (@excludeId IS NULL OR CategoryId <> @excludeId)";

        var count = await _db.ExecuteScalarAsync<int>(sql, new
        {
            name,
            parentCategoryId,
            excludeId
        });

        return count > 0;
    }


}
