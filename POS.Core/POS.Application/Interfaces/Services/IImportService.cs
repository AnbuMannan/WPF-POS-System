using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface IImportService
{
    Task<ImportResultDto> ImportProductsAsync(Stream fileStream);
}

