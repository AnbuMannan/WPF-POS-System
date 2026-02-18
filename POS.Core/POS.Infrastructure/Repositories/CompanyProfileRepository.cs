using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories
{
    public class CompanyProfileRepository : ICompanyProfileRepository
    {
        private readonly PosDbContext _db;

        public CompanyProfileRepository(PosDbContext db)
        {
            _db = db;
        }

        public async Task<CompanyProfile?> GetAsync()
        {
            return await _db.CompanyProfiles.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<CompanyProfile> CreateOrUpdateAsync(CompanyProfile entity)
        {
            var existing = await _db.CompanyProfiles.FirstOrDefaultAsync();

            if (existing == null)
            {
                entity.CreatedAt = DateTime.Now;
                entity.IsActive = true;
                await _db.CompanyProfiles.AddAsync(entity);
            }
            else
            {
                existing.Name = entity.Name;
                existing.Address = entity.Address;
                existing.City = entity.City;
                existing.State = entity.State;
                existing.PostalCode = entity.PostalCode;
                existing.Country = entity.Country;
                existing.Phone = entity.Phone;
                existing.Mobile = entity.Mobile;
                existing.Email = entity.Email;
                existing.Website = entity.Website;
                existing.GstNumber = entity.GstNumber;
                existing.PanNumber = entity.PanNumber;
                existing.LogoUrl = entity.LogoUrl;
                existing.CurrencySymbol = entity.CurrencySymbol;
                existing.CurrencyCode = entity.CurrencyCode;
                existing.ReceiptHeader = entity.ReceiptHeader;
                existing.ReceiptFooter = entity.ReceiptFooter;
                existing.UpdatedAt = DateTime.Now;

                entity = existing;
            }

            await _db.SaveChangesAsync();
            return entity;
        }
    }
}
