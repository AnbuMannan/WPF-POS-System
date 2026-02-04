using POS.Application.Interfaces.Services;

namespace POS.Infrastructure.Services;

public class ReturnService : IReturnService
{
    public Task<object> CreateReturnAsync(int saleId, object returnDto, string userId, CancellationToken cancellationToken = default)
        => Task.FromResult<object>(new { ReturnId = 1 });
}
