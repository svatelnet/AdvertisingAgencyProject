using AdvertisingAgencyProject.Data;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingAgencyProject.Services
{
    public class AdOrderStatusBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AdOrderStatusBackgroundService> _logger;

        public AdOrderStatusBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<AdOrderStatusBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateOrdersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка автоматического обновления статусов рекламы.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task UpdateOrdersAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var orders = await context.AdOrders.ToListAsync(stoppingToken);
            var hasChanges = false;

            foreach (var order in orders)
            {
                if (AdOrderWorkflowService.RefreshStatus(order))
                {
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await context.SaveChangesAsync(stoppingToken);
            }
        }
    }
}