using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;

namespace POS.Application.Services;

public class UomService : IUomService
{
    private readonly IUomRepository _repo;

    public UomService(IUomRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<Uom>> GetAllAsync()
        => await _repo.GetAllAsync();

    public async Task<Uom> GetByIdAsync(Guid id)
        => await _repo.GetByIdAsync(id);

    public async Task AddAsync(Uom uom)
    {
        Validate(uom);
        if (await _repo.CodeExistsAsync(uom.Code))
            throw new POS.Application.Exceptions.ValidationException("Code", "UOM code already exists");
        uom.UomId = Guid.NewGuid();
        uom.CreatedAt = DateTime.Now;
        uom.IsActive = true;
        await _repo.AddAsync(uom);
    }

    public async Task UpdateAsync(Uom uom)
    {
        Validate(uom);
        if (await _repo.CodeExistsAsync(uom.Code, uom.UomId))
            throw new POS.Application.Exceptions.ValidationException("Code", "UOM code already exists");
        uom.UpdatedAt = DateTime.Now;
        await _repo.UpdateAsync(uom);
    }

    public async Task DisableAsync(Guid id)
        => await _repo.DisableAsync(id);

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId)
        => await _repo.CodeExistsAsync(code, excludeId);

    private void Validate(Uom uom)
    {
        if (string.IsNullOrWhiteSpace(uom.Name))
            throw new Exception("UOM name is required");
        if (string.IsNullOrWhiteSpace(uom.Code))
            throw new Exception("UOM code is required");
        if (uom.Code.Length > 32)
            throw new Exception("UOM code too long");
        if (uom.Symbol != null && uom.Symbol.Length > 16)
            throw new Exception("UOM symbol too long");
        if (uom.DecimalPlaces < 0 || uom.DecimalPlaces > 6)
            throw new Exception("Decimal places must be between 0 and 6");
    }
}
