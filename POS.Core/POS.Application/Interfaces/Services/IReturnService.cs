namespace POS.Application.Interfaces.Services;

public interface IReturnService
{
    Task<object> CreateReturnAsync(int saleId, object returnDto, string userId, CancellationToken cancellationToken = default);
}
