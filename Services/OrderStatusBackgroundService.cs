using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventorySolution.Services
{
    public class OrderStatusBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<OrderStatusBackgroundService> _logger;

        public OrderStatusBackgroundService(IServiceProvider services,
                                           ILogger<OrderStatusBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Order Status Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Checking for order status updates...");

                try
                {
                    using var scope = _services.CreateScope();
                    var statusService = scope.ServiceProvider.GetRequiredService<IOrderStatusService>();
                    await statusService.UpdateOrderStatusesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating order statuses");
                }

                // Run every 10 minutes
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }

            _logger.LogInformation("Order Status Background Service is stopping.");
        }
    }
}