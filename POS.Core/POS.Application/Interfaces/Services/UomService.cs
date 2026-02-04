using POS.Application.Exceptions;
using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Services;

public class UomService : IUomService
{
    private readonly IUomRepository _repo;

    public UomService(IUomRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<UomDto>> GetAllAsync(bool includeInactive = false)
    {
        var list = await _repo.GetAllAsync(includeInactive);
        return (list ?? new List<Uom>()).Select(MapToDto).ToList();
    }

    public async Task<UomDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task AddAsync(UomDto dto)
    {
        var entity = MapToEntity(dto);
        Validate(entity);
        if (await _repo.CodeExistsAsync(entity.Code))
            throw new ValidationException("Code", "UOM code already exists");
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.IsActive = true;
        await _repo.AddAsync(entity);
    }

    public async Task UpdateAsync(UomDto dto)
    {
        var entity = MapToEntity(dto);
        Validate(entity);
        if (await _repo.CodeExistsAsync(entity.Code, entity.Id))
            throw new ValidationException("Code", "UOM code already exists");
        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity);
    }

    public async Task DisableAsync(Guid id)
        => await _repo.DisableAsync(id);

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId)
        => await _repo.CodeExistsAsync(code, excludeId);

    private static void Validate(Uom uom)
    {
        if (string.IsNullOrWhiteSpace(uom.Name))
            throw new ValidationException("Name", "UOM name is required");
        if (string.IsNullOrWhiteSpace(uom.Code))
            throw new ValidationException("Code", "UOM code is required");
        if (uom.Code.Length > 32)
            throw new ValidationException("Code", "UOM code too long");
        if (uom.Symbol != null && uom.Symbol.Length > 16)
            throw new ValidationException("Symbol", "UOM symbol too long");
        if (uom.DecimalPlaces < 0 || uom.DecimalPlaces > 6)
            throw new ValidationException("DecimalPlaces", "Decimal places must be between 0 and 6");
    }

    private static UomDto MapToDto(Uom e) => new UomDto
    {
        Id = e.Id,
        Name = e.Name,
        Code = e.Code,
        Symbol = e.Symbol,
        DecimalPlaces = e.DecimalPlaces,
        Description = e.Description,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    private static Uom MapToEntity(UomDto d)
    {
        var e = new Uom
        {
            Name = d.Name ?? string.Empty,
            Code = d.Code ?? string.Empty,
            Symbol = d.Symbol,
            DecimalPlaces = d.DecimalPlaces,
            Description = d.Description,
            IsActive = d.IsActive,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        };
        if (d.Id != Guid.Empty)
            e.Id = d.Id;
        return e;
    }
}
