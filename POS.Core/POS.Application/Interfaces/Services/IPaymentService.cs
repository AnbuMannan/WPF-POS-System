namespace POS.Application.Interfaces.Services;

public interface IPaymentService
{
    Task ValidatePaymentAsync(decimal amount, string method, CancellationToken cancellationToken = default);
}
