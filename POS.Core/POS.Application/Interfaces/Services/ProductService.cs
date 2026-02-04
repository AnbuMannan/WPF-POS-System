using POS.Application.Exceptions;
using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo) => _repo = repo;

    public async Task<ProductDto> GetByIdAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null ? null! : MapToDto(entity);
    }

    public async Task<ProductDto> GetByBarcodeAsync(string barcode)
    {
        var entity = await _repo.GetByBarcodeAsync(barcode);
        return entity == null ? null! : MapToDto(entity);
    }

    public async Task<List<ProductDto>> SearchAsync(string keyword)
        => (await _repo.SearchAsync(keyword)).Select(MapToDto).ToList();

    public async Task<List<Product>> SearchProductsAsync(string query)
        => await _repo.SearchAsync(string.IsNullOrWhiteSpace(query) ? "" : query);

    public async Task<Product?> GetProductByBarcodeAsync(string barcode)
        => await _repo.GetByBarcodeAsync(barcode ?? "");

    public async Task<bool> CheckStockAsync(long productId, decimal quantity)
    {
        // Inventory uses Guid productId; when mapped to long, implement stock check here.
        return await Task.FromResult(true);
    }

    public async Task AddAsync(ProductDto dto)
    {
        var product = MapToEntity(dto);
        Validate(product);

        if (!string.IsNullOrWhiteSpace(product.SKU) && await _repo.SKUExistsAsync(product.SKU))
            throw new ValidationException("SKU", "SKU already exists");
        if (!string.IsNullOrWhiteSpace(product.Barcode) && await _repo.BarcodeExistsAsync(product.Barcode))
            throw new ValidationException("Barcode", "Barcode already exists");

        product.CreatedAt = DateTime.Now;
        product.IsActive = true;
        await _repo.AddAsync(product);
    }

    public async Task UpdateAsync(ProductDto dto)
    {
        var product = MapToEntity(dto);
        Validate(product);

        if (!string.IsNullOrWhiteSpace(product.SKU) && await _repo.SKUExistsAsync(product.SKU, product.ProductId))
            throw new ValidationException("SKU", "SKU already exists");
        if (!string.IsNullOrWhiteSpace(product.Barcode) && await _repo.BarcodeExistsAsync(product.Barcode, product.ProductId))
            throw new ValidationException("Barcode", "Barcode already exists");

        product.UpdatedAt = DateTime.Now;
        await _repo.UpdateAsync(product);
    }

    public async Task DisableAsync(long id) => await _repo.DisableAsync(id);

    public async Task<List<ProductDto>> GetAllAsync(bool showInactive = false)
        => (await _repo.GetAllAsync(showInactive)).Select(MapToDto).ToList();

    public async Task<bool> SKUExistsAsync(string sku, long? excludeId)
        => await _repo.SKUExistsAsync(sku, excludeId);

    public async Task<bool> BarcodeExistsAsync(string barcode, long? excludeId)
        => await _repo.BarcodeExistsAsync(barcode, excludeId);

    private static void Validate(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
            throw new Exception("Product name required");
        if (product.SellingPrice <= 0)
            throw new Exception("Selling price must be > 0");
        if (product.TaxProfileId <= 0)
            throw new Exception("Valid Tax Profile is mandatory");
        if (string.IsNullOrWhiteSpace(product.HSNCode))
            throw new Exception("HSN Code is required for GST compliance");
    }

    private static ProductDto MapToDto(Product entity) => new ProductDto
    {
        ProductId = entity.ProductId,
        Name = entity.Name,
        SKU = entity.SKU,
        Barcode = entity.Barcode,
        Description = entity.Description,
        CategoryId = entity.CategoryId,
        CategoryName = entity.Category?.Name,
        BrandId = entity.BrandId,
        BrandName = entity.Brand?.Name,
        Unit = entity.Unit,
        CostPrice = entity.CostPrice,
        SellingPrice = entity.SellingPrice,
        MRP = entity.MRP,
        HSNCode = entity.HSNCode,
        TaxProfileId = entity.TaxProfileId,
        IsWeighable = entity.IsWeighable,
        IsManufactured = entity.IsManufactured,
        IsActive = entity.IsActive,
        RowVersion = entity.RowVersion == default ? null : BitConverter.GetBytes(entity.RowVersion.Ticks),
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedBy = entity.UpdatedBy
    };

    private static Product MapToEntity(ProductDto dto) => new Product
    {
        ProductId = dto.ProductId,
        Name = dto.Name,
        SKU = dto.SKU,
        Barcode = dto.Barcode,
        Description = dto.Description,
        CategoryId = dto.CategoryId,
        BrandId = dto.BrandId,
        Unit = dto.Unit,
        CostPrice = dto.CostPrice,
        SellingPrice = dto.SellingPrice,
        MRP = dto.MRP,
        HSNCode = dto.HSNCode,
        TaxProfileId = dto.TaxProfileId,
        IsWeighable = dto.IsWeighable,
        IsManufactured = dto.IsManufactured,
        IsActive = dto.IsActive,
        RowVersion = dto.RowVersion == null || dto.RowVersion.Length < 8 ? default : new DateTime(BitConverter.ToInt64(dto.RowVersion, 0)),
        CreatedAt = dto.CreatedAt,
        UpdatedAt = dto.UpdatedAt,
        CreatedBy = dto.CreatedBy,
        UpdatedBy = dto.UpdatedBy
    };
}
