using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using POS.Application.Exceptions;

namespace POS.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }

    public async Task<Product> GetByIdAsync(Guid id)
        => await _repo.GetByIdAsync(id);

    public async Task<Product> GetByBarcodeAsync(string barcode)
        => await _repo.GetByBarcodeAsync(barcode);

    public async Task<List<Product>> SearchAsync(string keyword)
        => await _repo.SearchAsync(keyword);

    public async Task AddAsync(Product product)
    {
        Validate(product);

        // 🔥 UNIQUE CHECKS
        if (!string.IsNullOrWhiteSpace(product.SKU))
        {
            if (await _repo.SKUExistsAsync(product.SKU))
                throw new Exceptions.ValidationException("SKU", "SKU already exists");
        }

        if (!string.IsNullOrWhiteSpace(product.Barcode))
        {
            if (await _repo.BarcodeExistsAsync(product.Barcode))
                throw new Exceptions.ValidationException("Barcode", "Barcode already exists");
        }

        product.ProductId = Guid.NewGuid();
        product.CreatedAt = DateTime.Now;
        
        product.IsActive = true;

        await _repo.AddAsync(product);
    }


    public async Task UpdateAsync(Product product)
    {
        Validate(product);

        // 🔥 UNIQUE CHECKS (exclude current product)
        if (!string.IsNullOrWhiteSpace(product.SKU))
        {
            if (await _repo.SKUExistsAsync(product.SKU, product.ProductId))
                throw new Exceptions.ValidationException("SKU", "SKU already exists");
        }

        if (!string.IsNullOrWhiteSpace(product.Barcode))
        {
            if (await _repo.BarcodeExistsAsync(product.Barcode, product.ProductId))
                throw new Exceptions.ValidationException("Barcode", "Barcode already exists");
        }

        product.UpdatedAt = DateTime.Now;
        await _repo.UpdateAsync(product);
    }


    public async Task DisableAsync(Guid id)
        => await _repo.DisableAsync(id);

    private void Validate(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
            throw new Exception("Product name required");

        if (product.SellingPrice <= 0)
            throw new Exception("Selling price must be > 0");

        if (product.TaxProfileId == Guid.Empty)
            throw new Exception("Valid Tax Profile is mandatory");

        if (string.IsNullOrWhiteSpace(product.HSNCode))
            throw new Exception("HSN Code is required for GST compliance");

    }
    public async Task<List<Product>> GetAllAsync([FromQuery] bool showInactive = false)
        => await _repo.GetAllAsync(showInactive);
    public async Task<bool> SKUExistsAsync(string sku, Guid? excludeId)
    => await _repo.SKUExistsAsync(sku, excludeId);

    public async Task<bool> BarcodeExistsAsync(string barcode, Guid? excludeId)
        => await _repo.BarcodeExistsAsync(barcode, excludeId);

}
