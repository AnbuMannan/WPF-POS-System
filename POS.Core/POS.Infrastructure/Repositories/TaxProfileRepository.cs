using Dapper;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using System.Data;

namespace POS.Infrastructure.Repositories;

public class TaxProfileRepository : ITaxProfileRepository
{
    private readonly IDbConnection _db;

    public TaxProfileRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<List<TaxProfile>> GetAllAsync()
        => (await _db.QueryAsync<TaxProfile>(
            "SELECT  TaxProfileId AS Id,Name  FROM TaxProfiles WHERE IsActive=1")).ToList();

    public async Task<TaxProfile> GetByIdAsync(Guid id)
        => await _db.QueryFirstOrDefaultAsync<TaxProfile>(
            "SELECT * FROM TaxProfiles WHERE TaxProfileId=@id", new { id });

    public async Task AddAsync(TaxProfile tax)
    {
        await _db.ExecuteAsync(@"
        INSERT INTO TaxProfiles
        (TaxProfileId,Name,CGST,SGST,IGST,Cess,IsActive)
        VALUES
        (@TaxProfileId,@Name,@CGST,@SGST,@IGST,@Cess,@IsActive)", tax);
    }

    public async Task UpdateAsync(TaxProfile tax)
    {
        await _db.ExecuteAsync(@"
        UPDATE TaxProfiles SET
        Name=@Name,
        CGST=@CGST,
        SGST=@SGST,
        IGST=@IGST,
        Cess=@Cess
        WHERE TaxProfileId=@TaxProfileId", tax);
    }

    public async Task DisableAsync(Guid id)
        => await _db.ExecuteAsync(
            "UPDATE TaxProfiles SET IsActive=0 WHERE TaxProfileId=@id", new { id });
}
