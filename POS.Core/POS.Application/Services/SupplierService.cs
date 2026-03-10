using POS.Application.Exceptions;
using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _repo;

    public SupplierService(ISupplierRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<SupplierDto>> GetAllAsync(bool includeInactive = false)
    {
        var list = await _repo.GetAllAsync(includeInactive);
        return (list ?? new List<Supplier>()).Select(MapToDto).ToList();
    }

    public async Task<SupplierDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task AddAsync(SupplierDto dto)
    {
        var entity = MapToEntity(dto);
        Validate(entity);
        
        if (await _repo.CheckCodeExistsAsync(entity.Code, null))
            throw new ValidationException("Code", "Supplier code already exists.");
        
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.Now;
        entity.IsActive = true;
        await _repo.AddAsync(entity);
    }

    public async Task UpdateAsync(SupplierDto dto)
    {
        var entity = MapToEntity(dto);
        Validate(entity);
        
        if (await _repo.CheckCodeExistsAsync(entity.Code, entity.Id))
            throw new ValidationException("Code", "Supplier code already exists.");
        
        entity.UpdatedAt = DateTime.Now;
        await _repo.UpdateAsync(entity);
    }

    public async Task DisableAsync(Guid id)
        => await _repo.DisableAsync(id);

    public async Task<bool> CheckCodeExistsAsync(string code, Guid? excludeId)
        => await _repo.CheckCodeExistsAsync(code, excludeId);

    private static void Validate(Supplier s)
    {
        if (string.IsNullOrWhiteSpace(s.Name))
            throw new ValidationException("Name", "Supplier name is required.");
        if (s.Name.Length > 200)
            throw new ValidationException("Name", "Supplier name too long.");
        
        if (string.IsNullOrWhiteSpace(s.Code))
            throw new ValidationException("Code", "Supplier code is required.");
        if (s.Code.Length > 50)
            throw new ValidationException("Code", "Supplier code too long.");

        if (!string.IsNullOrWhiteSpace(s.Mobile) && s.Mobile.Length > 20)
            throw new ValidationException("Mobile", "Mobile number too long.");
        
        if (!string.IsNullOrWhiteSpace(s.Email) && s.Email.Length > 256)
            throw new ValidationException("Email", "Email too long.");
    }

    private static SupplierDto MapToDto(Supplier e) => new SupplierDto
    {
        Id = e.Id,
        Name = e.Name,
        Code = e.Code,
        ContactPerson = e.ContactPerson,
        Mobile = e.Mobile,
        Email = e.Email,
        Address = e.Address,
        GstVatNumber = e.GstVatNumber,
        CreditPeriodDays = e.CreditPeriodDays,
        CreditLimit = e.CreditLimit,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    private static Supplier MapToEntity(SupplierDto d)
    {
        var e = new Supplier
        {
            Name = d.Name ?? string.Empty,
            Code = d.Code ?? string.Empty,
            ContactPerson = d.ContactPerson,
            Mobile = d.Mobile,
            Email = d.Email,
            Address = d.Address,
            GstVatNumber = d.GstVatNumber,
            CreditPeriodDays = d.CreditPeriodDays,
            CreditLimit = d.CreditLimit,
            IsActive = d.IsActive,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        };
        if (d.Id != Guid.Empty)
            e.Id = d.Id;
        return e;
    }
}
