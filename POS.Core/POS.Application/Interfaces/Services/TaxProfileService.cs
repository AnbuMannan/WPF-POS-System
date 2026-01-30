using POS.Application.Exceptions;
using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Services;

public class TaxProfileService : ITaxProfileService
{
    private readonly ITaxProfileRepository _repo;

    public TaxProfileService(ITaxProfileRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<TaxProfileDto>> GetAllAsync(bool includeInactive = false)
    {
        var list = await _repo.GetAllAsync(includeInactive);
        return (list ?? new List<TaxProfile>()).Select(MapToDto).ToList();
    }

    public async Task<TaxProfileDto> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null ? null! : MapToDto(entity);
    }

    public async Task AddAsync(TaxProfileDto dto)
    {
        var entity = MapToEntity(dto);
        Validate(entity);
        entity.IsActive = true;
        await _repo.AddAsync(entity);
    }

    public async Task UpdateAsync(TaxProfileDto dto)
    {
        var entity = MapToEntity(dto);
        Validate(entity);
        await _repo.UpdateAsync(entity);
    }

    public async Task DisableAsync(int id)
        => await _repo.DisableAsync(id);

    private static void Validate(TaxProfile tax)
    {
        if (string.IsNullOrWhiteSpace(tax.Name))
            throw new ValidationException("Name", "Tax profile name required");
        if (tax.CGST < 0 || tax.SGST < 0 || tax.IGST < 0 || tax.Cess < 0)
            throw new ValidationException("CGST", "Tax rates cannot be negative");
        if ((tax.CGST + tax.SGST + tax.IGST + tax.Cess) > 100)
            throw new ValidationException("CGST", "Invalid GST total percentage");
    }

    private static TaxProfileDto MapToDto(TaxProfile e) => new TaxProfileDto
    {
        TaxProfileId = e.TaxProfileId,
        Name = e.Name,
        CGST = e.CGST,
        SGST = e.SGST,
        IGST = e.IGST,
        Cess = e.Cess,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    private static TaxProfile MapToEntity(TaxProfileDto d) => new TaxProfile
    {
        TaxProfileId = d.TaxProfileId,
        Name = d.Name,
        CGST = d.CGST,
        SGST = d.SGST,
        IGST = d.IGST,
        Cess = d.Cess,
        IsActive = d.IsActive,
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt
    };
}
