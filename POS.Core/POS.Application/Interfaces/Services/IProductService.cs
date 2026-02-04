using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface IProductService
{
    Task<ProductDto> GetByIdAsync(long id);
    Task<ProductDto> GetByBarcodeAsync(string barcode);
    Task<List<ProductDto>> SearchAsync(string keyword);
    Task<List<Product>> SearchProductsAsync(string query);
    Task<Product?> GetProductByBarcodeAsync(string barcode);
    Task<bool> CheckStockAsync(long productId, decimal quantity);
    Task AddAsync(ProductDto product);
    Task UpdateAsync(ProductDto product);
    Task DisableAsync(long id);
    Task<List<ProductDto>> GetAllAsync(bool showInactive);
    Task<bool> SKUExistsAsync(string sku, long? excludeId);
    Task<bool> BarcodeExistsAsync(string barcode, long? excludeId);

}

