using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Services
{
    public class CompanyProfileService : ICompanyProfileService
    {
        private readonly ICompanyProfileRepository _repo;

        public CompanyProfileService(ICompanyProfileRepository repo)
        {
            _repo = repo;
        }

        public async Task<CompanyProfileDto?> GetAsync()
        {
            var entity = await _repo.GetAsync();
            return entity == null ? null : MapToDto(entity);
        }

        public async Task<CompanyProfileDto> SaveAsync(UpdateCompanyProfileDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Company name is required");

            var entity = new CompanyProfile
            {
                Name = dto.Name,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                PostalCode = dto.PostalCode,
                Country = dto.Country,
                Phone = dto.Phone,
                Mobile = dto.Mobile,
                Email = dto.Email,
                Website = dto.Website,
                GstNumber = dto.GstNumber,
                PanNumber = dto.PanNumber,
                LogoUrl = dto.LogoUrl,
                CurrencySymbol = dto.CurrencySymbol ?? "₹",
                CurrencyCode = dto.CurrencyCode ?? "INR",
                ReceiptHeader = dto.ReceiptHeader,
                ReceiptFooter = dto.ReceiptFooter
            };

            var saved = await _repo.CreateOrUpdateAsync(entity);
            return MapToDto(saved);
        }

        private static CompanyProfileDto MapToDto(CompanyProfile entity)
        {
            return new CompanyProfileDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Address = entity.Address,
                City = entity.City,
                State = entity.State,
                PostalCode = entity.PostalCode,
                Country = entity.Country,
                Phone = entity.Phone,
                Mobile = entity.Mobile,
                Email = entity.Email,
                Website = entity.Website,
                GstNumber = entity.GstNumber,
                PanNumber = entity.PanNumber,
                LogoUrl = entity.LogoUrl,
                CurrencySymbol = entity.CurrencySymbol,
                CurrencyCode = entity.CurrencyCode,
                ReceiptHeader = entity.ReceiptHeader,
                ReceiptFooter = entity.ReceiptFooter
            };
        }
    }
}
