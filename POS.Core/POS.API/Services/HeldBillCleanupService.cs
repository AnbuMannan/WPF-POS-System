using POS.Application.Interfaces.Services;

namespace POS.API.Services;

public class HeldBillCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public HeldBillCleanupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                using var scope = _serviceProvider.CreateScope();
                var heldBillService = scope.ServiceProvider.GetRequiredService<IHeldBillService>();
                var heldBills = await heldBillService.GetHeldBillsAsync(null, stoppingToken);
                // Optional: delete expired held bills by Id
                foreach (var bill in heldBills)
                {
                    // if (IsExpired(bill)) await heldBillService.DeleteAsync(bill.Id, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // Log and continue
            }
        }
    }
}
