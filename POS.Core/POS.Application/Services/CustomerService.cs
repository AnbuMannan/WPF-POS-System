using AutoMapper;
using POS.Application.Exceptions;
using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repo;
    private readonly IMapper _mapper;

    public CustomerService(ICustomerRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<CustomerDto>> GetAllAsync()
    {
        var entities = await _repo.GetAllAsync();
        return _mapper.Map<List<CustomerDto>>(entities);
    }

    public async Task<CustomerDto> GetByIdAsync(string id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null ? null! : _mapper.Map<CustomerDto>(entity);
    }

    public async Task AddAsync(CustomerDto dto)
    {
        var customer = _mapper.Map<Customer>(dto);
        Validate(customer);

        if (string.IsNullOrWhiteSpace(customer.CustomerId))
            customer.CustomerId = Guid.NewGuid().ToString();

        if (!string.IsNullOrWhiteSpace(customer.Phone) && await _repo.CheckPhoneExistsAsync(customer.Phone, null))
            throw new ValidationException("Phone", "Phone number already exists.");

        customer.IsActive = true;
        customer.CreatedAt = DateTime.Now;
        await _repo.AddAsync(customer);
    }

    public async Task UpdateAsync(CustomerDto dto)
    {
        var customer = _mapper.Map<Customer>(dto);
        Validate(customer);

        if (!string.IsNullOrWhiteSpace(customer.Phone) && await _repo.CheckPhoneExistsAsync(customer.Phone, customer.CustomerId))
            throw new ValidationException("Phone", "Phone number already exists.");

        customer.UpdatedAt = DateTime.Now;
        await _repo.UpdateAsync(customer);
    }

    public async Task DisableAsync(string id) => await _repo.DisableAsync(id);

    public async Task<bool> CheckPhoneExistsAsync(string phone, string? excludeId)
    {
        return await _repo.CheckPhoneExistsAsync(phone, excludeId);
    }

    private void Validate(Customer customer)
    {
        if (string.IsNullOrWhiteSpace(customer.FirstName))
            throw new ValidationException("FirstName", "First name is required.");
        if (string.IsNullOrWhiteSpace(customer.LastName))
            throw new ValidationException("LastName", "Last name is required.");
    }
}
