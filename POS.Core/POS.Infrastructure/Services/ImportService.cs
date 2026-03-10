using System.Globalization;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Infrastructure.Data;
using POS.Shared.Models;

namespace POS.Infrastructure.Services;

public class ImportService : IImportService
{
    private readonly PosDbContext _db;

    public ImportService(PosDbContext db)
    {
        _db = db;
    }

    public async Task<ImportResultDto> ImportProductsAsync(Stream fileStream)
    {
        var result = new ImportResultDto();

        using var workbook = new XLWorkbook(fileStream);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet == null)
        {
            result.Errors.Add("No worksheet found in the uploaded file.");
            return result;
        }

        var headerRow = worksheet.FirstRowUsed();
        if (headerRow == null)
        {
            result.Errors.Add("Header row is missing in the uploaded file.");
            return result;
        }

        var headers = headerRow.Cells().ToDictionary(
            c => c.GetString().Trim(),
            c => c.Address.ColumnNumber,
            StringComparer.OrdinalIgnoreCase);

        string[] requiredHeaders =
        {
            "Name", "Code", "Barcode", "Price", "Cost", "Stock", "Category", "Brand", "UOM"
        };

        var missingHeaders = requiredHeaders.Where(h => !headers.ContainsKey(h)).ToList();
        if (missingHeaders.Any())
        {
            result.Errors.Add("Missing required headers: " + string.Join(", ", missingHeaders));
            return result;
        }

        var categoryLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var brandLookup = new Dictionary<string, int?>(StringComparer.OrdinalIgnoreCase);
        var uomLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var skuLookup = new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase);

        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                result.RowsProcessed++;
                var rowNumber = row.RowNumber();

                try
                {
                    var name = row.Cell(headers["Name"]).GetString().Trim();
                    var code = row.Cell(headers["Code"]).GetString().Trim();
                    var barcode = row.Cell(headers["Barcode"]).GetString().Trim();
                    var priceString = row.Cell(headers["Price"]).GetString().Trim();
                    var costString = row.Cell(headers["Cost"]).GetString().Trim();
                    var stockString = row.Cell(headers["Stock"]).GetString().Trim();
                    var categoryName = row.Cell(headers["Category"]).GetString().Trim();
                    var brandName = row.Cell(headers["Brand"]).GetString().Trim();
                    var uomCode = row.Cell(headers["UOM"]).GetString().Trim();

                    if (string.IsNullOrWhiteSpace(code))
                    {
                        result.Errors.Add($"Row {rowNumber}: Code is required.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        result.Errors.Add($"Row {rowNumber}: Name is required.");
                        continue;
                    }

                    if (!decimal.TryParse(priceString, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                    {
                        result.Errors.Add($"Row {rowNumber}: Invalid Price value '{priceString}'.");
                        continue;
                    }

                    if (!decimal.TryParse(costString, NumberStyles.Any, CultureInfo.InvariantCulture, out var cost))
                    {
                        result.Errors.Add($"Row {rowNumber}: Invalid Cost value '{costString}'.");
                        continue;
                    }

                    if (!decimal.TryParse(stockString, NumberStyles.Any, CultureInfo.InvariantCulture, out var stock))
                    {
                        result.Errors.Add($"Row {rowNumber}: Invalid Stock value '{stockString}'.");
                        continue;
                    }

                    var categoryId = await GetOrCreateCategoryIdAsync(categoryName, categoryLookup);
                    int? brandId = null;
                    if (!string.IsNullOrWhiteSpace(brandName))
                    {
                        brandId = await GetOrCreateBrandIdAsync(brandName, brandLookup);
                    }

                    var unit = await GetOrCreateUomCodeAsync(uomCode, uomLookup);

                    if (!skuLookup.TryGetValue(code, out var product))
                    {
                        product = await _db.Products.FirstOrDefaultAsync(p => p.SKU == code);
                        if (product != null)
                        {
                            skuLookup[code] = product;
                        }
                    }

                    if (product == null)
                    {
                        product = new Product
                        {
                            Name = name,
                            SKU = code,
                            Barcode = string.IsNullOrWhiteSpace(barcode) ? null : barcode,
                            CategoryId = categoryId,
                            BrandId = brandId,
                            Unit = unit,
                            CostPrice = cost,
                            SellingPrice = price,
                            MRP = price,
                            HSNCode = null,
                            IsWeighable = false,
                            IsManufactured = false,
                            IsActive = true,
                            CreatedAt = DateTime.Now,
                            CreatedBy = "Import"
                        };

                        _db.Products.Add(product);
                        await _db.SaveChangesAsync();

                        skuLookup[code] = product;
                    }
                    else
                    {
                        product.Name = name;
                        product.Barcode = string.IsNullOrWhiteSpace(barcode) ? product.Barcode : barcode;
                        product.CategoryId = categoryId;
                        product.BrandId = brandId;
                        product.Unit = unit;
                        product.CostPrice = cost;
                        product.SellingPrice = price;
                        product.MRP = price;
                        product.UpdatedAt = DateTime.Now;
                        product.UpdatedBy = "Import";

                        _db.Products.Update(product);
                        await _db.SaveChangesAsync();
                    }

                    if (stock != 0)
                    {
                        var summary = await _db.StockSummaries.FirstOrDefaultAsync(s => s.ProductId == product.ProductId);
                        if (summary == null)
                        {
                            summary = new StockSummary
                            {
                                ProductId = product.ProductId,
                                AvailableStock = stock,
                                LastUpdated = DateTime.Now
                            };
                            _db.StockSummaries.Add(summary);
                        }
                        else
                        {
                            summary.AvailableStock = stock;
                            summary.LastUpdated = DateTime.Now;
                            _db.StockSummaries.Update(summary);
                        }

                        await _db.SaveChangesAsync();
                    }

                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Row {rowNumber}: {ex.Message}");
                }
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            result.Errors.Add("Fatal error during import: " + ex.Message);
        }

        return result;
    }

    private async Task<int> GetOrCreateCategoryIdAsync(string categoryName, Dictionary<string, int> cache)
    {
        var key = string.IsNullOrWhiteSpace(categoryName) ? "Uncategorized" : categoryName.Trim();

        if (cache.TryGetValue(key, out var id))
            return id;

        var normalized = key.Trim();
        var existing = await _db.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == normalized.ToLower());

        if (existing == null)
        {
            existing = new Category
            {
                Name = normalized,
                Code = normalized.Replace(" ", string.Empty),
                DisplayOrder = 0,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            _db.Categories.Add(existing);
            await _db.SaveChangesAsync();
        }

        cache[key] = existing.CategoryId;
        return existing.CategoryId;
    }

    private async Task<int?> GetOrCreateBrandIdAsync(string brandName, Dictionary<string, int?> cache)
    {
        if (string.IsNullOrWhiteSpace(brandName))
            return null;

        var key = brandName.Trim();
        if (cache.TryGetValue(key, out var cachedId))
            return cachedId;

        var normalized = key.Trim();
        var existing = await _db.Brands
            .FirstOrDefaultAsync(b => b.Name.ToLower() == normalized.ToLower());

        if (existing == null)
        {
            existing = new Brand
            {
                Name = normalized,
                Code = normalized.Replace(" ", string.Empty),
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            _db.Brands.Add(existing);
            await _db.SaveChangesAsync();
        }

        cache[key] = existing.BrandId;
        return existing.BrandId;
    }

    private async Task<string> GetOrCreateUomCodeAsync(string uom, Dictionary<string, string> cache)
    {
        var key = string.IsNullOrWhiteSpace(uom) ? "PCS" : uom.Trim();

        if (cache.TryGetValue(key, out var cached))
            return cached;

        var normalized = key.Trim();
        var existing = await _db.Uoms
            .FirstOrDefaultAsync(u => u.Code.ToLower() == normalized.ToLower());

        if (existing == null)
        {
            existing = new Uom
            {
                Id = Guid.NewGuid(),
                Name = normalized,
                Code = normalized,
                Symbol = normalized,
                DecimalPlaces = 0,
                Description = null,
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            _db.Uoms.Add(existing);
            await _db.SaveChangesAsync();
        }

        cache[key] = existing.Code;
        return existing.Code;
    }
}

