using MesEnterprise.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MesEnterprise.Services.BackgroundServices;

public class EquipmentHourTrackingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EquipmentHourTrackingService> _logger;
    private readonly TimeSpan _initialDelay;
    private readonly TimeSpan _updateInterval = TimeSpan.FromHours(1);

    public EquipmentHourTrackingService(
        IServiceProvider serviceProvider,
        ILogger<EquipmentHourTrackingService> logger,
        IOptions<BackgroundServiceOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _initialDelay = options.Value.InitialDelay;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EquipmentHourTrackingService starting");

        if (_initialDelay > TimeSpan.Zero)
        {
            await Task.Delay(_initialDelay, stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateEquipmentHoursAsync(stoppingToken);
                await Task.Delay(_updateInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("EquipmentHourTrackingService stopping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EquipmentHourTrackingService");
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }
    }

    private async Task UpdateEquipmentHoursAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MesDbContext>();

        // Update OreFunctionare for all active lines' equipment
        var activeLines = await db.LineStatuses
            .Where(ls => ls.Status == "Running" || ls.Status == "Production")
            .Include(ls => ls.Line)
            .ToListAsync(cancellationToken);

        if (activeLines.Any())
        {
            // Get equipment for active lines
            var lineIds = activeLines.Select(ls => ls.LineId).ToList();
            var equipments = await db.Equipments
                .Where(e => e.LineId.HasValue && lineIds.Contains(e.LineId.Value))
                .ToListAsync(cancellationToken);

            foreach (var equipment in equipments)
            {
                equipment.OperatingHours += 1.0m; // Add 1 hour
            }

            await db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Updated operating hours for {equipments.Count} equipment items on {activeLines.Count} active lines");
        }
    }
}
