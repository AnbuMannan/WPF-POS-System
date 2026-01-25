using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;

namespace POS.Application.Services;

public class TaxProfileService : ITaxProfileService
{
    private readonly ITaxProfileRepository _repo;

    public TaxProfileService(ITaxProfileRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<TaxProfile>> GetAllAsync()
        => await _repo.GetAllAsync();

    public async Task<TaxProfile> GetByIdAsync(Guid id)
        => await _repo.GetByIdAsync(id);

    public async Task AddAsync(TaxProfile taxProfile)
    {
        Validate(taxProfile);
        taxProfile.Id = Guid.NewGuid();
        taxProfile.IsActive = true;
        await _repo.AddAsync(taxProfile);
    }

    public async Task UpdateAsync(TaxProfile taxProfile)
    {
        Validate(taxProfile);
        await _repo.UpdateAsync(taxProfile);
    }

    public async Task DisableAsync(Guid id)
        => await _repo.DisableAsync(id);

    private void Validate(TaxProfile tax)
    {
        if (string.IsNullOrWhiteSpace(tax.Name))
            throw new Exception("Tax profile name required");

        if (tax.CGST < 0 || tax.SGST < 0 || tax.IGST < 0 || tax.Cess < 0)
            throw new Exception("Tax rates cannot be negative");

        if ((tax.CGST + tax.SGST + tax.IGST + tax.Cess) > 100)
            throw new Exception("Invalid GST total percentage");
    }
}
