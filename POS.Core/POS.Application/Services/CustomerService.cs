using POS.Application.Exceptions;
using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repo;

    public CustomerService(ICustomerRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<CustomerDto>> GetAllAsync(bool includeInactive = false)
    {
        var list = await _repo.GetAllAsync(includeInactive);
        return (list ?? new List<Customer>()).Select(MapToDto).ToList();
    }

    public async Task<List<Customer>> SearchCustomersAsync(string query)
    {
        return await _repo.SearchAsync(query ?? "");
    }

    public async Task<Customer> CreateCustomerAsync(CreateCustomerDto dto)
    {
        var entity = new Customer
        {
            Id = Guid.NewGuid(),
            Name = dto.Name ?? "",
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            LoyaltyPoints = 0,
            IsActive = true,
            CreatedAt = DateTime.Now
        };
        Validate(entity);
        if (!string.IsNullOrWhiteSpace(entity.Phone) && await _repo.CheckPhoneExistsAsync(entity.Phone, null))
            throw new ValidationException("Phone", "Phone number already exists.");
        await _repo.AddAsync(entity);
        return entity;
    }

    public async Task UpdateLoyaltyPointsAsync(Guid customerId, int points)
    {
        var entity = await _repo.GetByIdAsync(customerId);
        if (entity == null)
            throw new ValidationException("CustomerId", "Customer not found.");
        entity.LoyaltyPoints = points;
        entity.UpdatedAt = DateTime.Now;
        await _repo.UpdateAsync(entity);
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task AddAsync(CustomerDto dto)
    {
        var entity = MapToEntity(dto);
        Validate(entity);
        if (!string.IsNullOrWhiteSpace(entity.Phone) && await _repo.CheckPhoneExistsAsync(entity.Phone, null))
            throw new ValidationException("Phone", "Phone number already exists.");
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.Now;
        entity.IsActive = true;
        await _repo.AddAsync(entity);
    }

    public async Task UpdateAsync(CustomerDto dto)
    {
        var entity = MapToEntity(dto);
        Validate(entity);
        if (!string.IsNullOrWhiteSpace(entity.Phone) && await _repo.CheckPhoneExistsAsync(entity.Phone, entity.Id))
            throw new ValidationException("Phone", "Phone number already exists.");
        entity.UpdatedAt = DateTime.Now;
        await _repo.UpdateAsync(entity);
    }

    public async Task DisableAsync(Guid id)
        => await _repo.DisableAsync(id);

    public async Task<bool> CheckPhoneExistsAsync(string? phone, Guid? excludeId)
        => await _repo.CheckPhoneExistsAsync(phone, excludeId);

    private static void Validate(Customer c)
    {
        if (string.IsNullOrWhiteSpace(c.Name))
            throw new ValidationException("Name", "Customer name is required.");
        if (c.Name.Length > 200)
            throw new ValidationException("Name", "Customer name too long.");
    }

    private static CustomerDto MapToDto(Customer e) => new CustomerDto
    {
        Id = e.Id,
        Name = e.Name,
        Phone = e.Phone,
        Email = e.Email,
        Address = e.Address,
        LoyaltyPoints = e.LoyaltyPoints,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    private static Customer MapToEntity(CustomerDto d)
    {
        var e = new Customer
        {
            Name = d.Name ?? string.Empty,
            Phone = d.Phone,
            Email = d.Email,
            Address = d.Address,
            LoyaltyPoints = d.LoyaltyPoints,
            IsActive = d.IsActive,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        };
        if (d.Id != Guid.Empty)
            e.Id = d.Id;
        return e;
    }
}
