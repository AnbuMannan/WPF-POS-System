using AutoMapper;
using POS.Application.Exceptions;
using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Services;

public class BrandService : IBrandService
{
    private readonly IBrandRepository _repo;
    private readonly IMapper _mapper;

    public BrandService(IBrandRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<BrandDto>> GetAllAsync(bool includeInactive = false)
    {
        var entities = await _repo.GetAllAsync(includeInactive);
        return _mapper.Map<List<BrandDto>>(entities);
    }

    public async Task<BrandDto> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null ? null! : _mapper.Map<BrandDto>(entity);
    }

    public async Task AddAsync(BrandDto dto)
    {
        var brand = _mapper.Map<Brand>(dto);
        Validate(brand);

        if (await _repo.CheckNameExistsAsync(brand.Name, null))
            throw new ValidationException("Name", "Brand name already exists.");

        brand.IsActive = true;
        brand.CreatedAt = DateTime.Now;
        await _repo.AddAsync(brand);
    }

    public async Task UpdateAsync(BrandDto dto)
    {
        var brand = _mapper.Map<Brand>(dto);
        Validate(brand);

        if (await _repo.CheckNameExistsAsync(brand.Name, brand.BrandId))
            throw new ValidationException("Name", "Brand name already exists.");

        brand.UpdatedAt = DateTime.Now;
        await _repo.UpdateAsync(brand);
    }

    public async Task DisableAsync(int id) => await _repo.DisableAsync(id);

    public async Task<bool> CheckNameExistsAsync(string name, int? excludeId)
    {
        return await _repo.CheckNameExistsAsync(name, excludeId);
    }

    private void Validate(Brand brand)
    {
        if (string.IsNullOrWhiteSpace(brand.Name))
            throw new ValidationException("Name", "Brand name is required.");
    }
}
