using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;

namespace POS.Application.Services;

public class BrandService : IBrandService
{
    private readonly IBrandRepository _repo;

    public BrandService(IBrandRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<Brand>> GetAllAsync()
        => await _repo.GetAllAsync();

    public async Task<Brand> GetByIdAsync(Guid id)
        => await _repo.GetByIdAsync(id);

    public async Task AddAsync(Brand brand)
    {
        if (string.IsNullOrWhiteSpace(brand.Name))
            throw new Exception("Brand name is required");

        brand.Id = Guid.NewGuid();
        brand.IsActive = true;
        await _repo.AddAsync(brand);
    }

    public async Task UpdateAsync(Brand brand)
    {
        if (string.IsNullOrWhiteSpace(brand.Name))
            throw new Exception("Brand name is required");

        await _repo.UpdateAsync(brand);
    }

    public async Task DisableAsync(Guid id)
        => await _repo.DisableAsync(id);
}
