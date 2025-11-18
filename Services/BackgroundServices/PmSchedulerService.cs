using MesEnterprise.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MesEnterprise.Services.BackgroundServices;

public class PmSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PmSchedulerService> _logger;
    private readonly TimeSpan _initialDelay;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(4);

    public PmSchedulerService(
        IServiceProvider serviceProvider,
        ILogger<PmSchedulerService> logger,
        IOptions<BackgroundServiceOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _initialDelay = options.Value.InitialDelay;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PmSchedulerService starting");

        if (_initialDelay > TimeSpan.Zero)
        {
            await Task.Delay(_initialDelay, stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckPmSchedulesAsync(stoppingToken);
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("PmSchedulerService stopping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PmSchedulerService");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }

    private async Task CheckPmSchedulesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MesDbContext>();

        var duePlans = await db.PreventiveMaintenancePlans
            .Where(p => p.NextDueDate <= DateTime.UtcNow && p.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var plan in duePlans)
        {
            _logger.LogInformation($"PM Plan {plan.Id} is due");
            // TODO: Create PM work order or notification
        }

        _logger.LogInformation($"Checked PM schedules: {duePlans.Count} plans due");
    }
}
