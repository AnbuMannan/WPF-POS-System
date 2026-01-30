using Dapper;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using System.Data;

namespace POS.Infrastructure.Repositories;

public class TaxProfileRepository : ITaxProfileRepository
{
    private readonly IDbConnection _db;

    public TaxProfileRepository(IDbConnection db) => _db = db;

    public async Task<List<TaxProfile>> GetAllAsync(bool includeInactive = false)
    {
        var sql = includeInactive
            ? "SELECT TaxProfileId, Name, CGST, SGST, IGST, Cess, IsActive, CreatedAt, UpdatedAt FROM TaxProfiles"
            : "SELECT TaxProfileId, Name, CGST, SGST, IGST, Cess, IsActive, CreatedAt, UpdatedAt FROM TaxProfiles WHERE IsActive = 1";
        return (await _db.QueryAsync<TaxProfile>(sql)).ToList();
    }

    public async Task<TaxProfile> GetByIdAsync(int id)
        => await _db.QueryFirstOrDefaultAsync<TaxProfile>(
            "SELECT TaxProfileId, Name, CGST, SGST, IGST, Cess, IsActive, CreatedAt, UpdatedAt FROM TaxProfiles WHERE TaxProfileId = @id", new { id });

    public async Task AddAsync(TaxProfile tax)
    {
        await _db.ExecuteAsync(@"
            INSERT INTO TaxProfiles (Name, CGST, SGST, IGST, Cess, IsActive)
            VALUES (@Name, @CGST, @SGST, @IGST, @Cess, @IsActive)", tax);
    }

    public async Task UpdateAsync(TaxProfile tax)
    {
        await _db.ExecuteAsync(@"
            UPDATE TaxProfiles SET Name = @Name, CGST = @CGST, SGST = @SGST, IGST = @IGST, Cess = @Cess, UpdatedAt = CURRENT_TIMESTAMP
            WHERE TaxProfileId = @TaxProfileId", tax);
    }

    public async Task DisableAsync(int id)
        => await _db.ExecuteAsync(
            "UPDATE TaxProfiles SET IsActive = 0, UpdatedAt = CURRENT_TIMESTAMP WHERE TaxProfileId = @id", new { id });
}
