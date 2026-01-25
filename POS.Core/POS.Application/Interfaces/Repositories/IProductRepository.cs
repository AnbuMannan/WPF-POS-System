using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<Product> GetByIdAsync(Guid id);
    Task<Product> GetByBarcodeAsync(string barcode);
    Task<List<Product>> SearchAsync(string keyword);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DisableAsync(Guid id);
    Task<List<Product>> GetAllAsync(bool showInactive);
    Task<bool> SKUExistsAsync(string sku, Guid? excludeId = null);
    Task<bool> BarcodeExistsAsync(string barcode, Guid? excludeId = null);

}
