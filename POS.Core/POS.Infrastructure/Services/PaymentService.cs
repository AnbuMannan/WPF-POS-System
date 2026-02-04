using POS.Application.Interfaces.Services;

namespace POS.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    public Task ValidatePaymentAsync(decimal amount, string method, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
