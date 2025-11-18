using MesEnterprise.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MesEnterprise.Services.BackgroundServices;

public class InventoryAlertService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InventoryAlertService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6);

    public InventoryAlertService(
        IServiceProvider serviceProvider,
        ILogger<InventoryAlertService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("InventoryAlertService starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckInventoryLevelsAsync(stoppingToken);
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("InventoryAlertService stopping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InventoryAlertService");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

    private async Task CheckInventoryLevelsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MesDbContext>();

        // Check spare parts
        var lowSpareParts = await db.SpareParts
            .Where(sp => sp.QuantityInStock <= sp.MinimumStock)
            .ToListAsync(cancellationToken);

        foreach (var part in lowSpareParts)
        {
            _logger.LogWarning($"Low stock alert: Spare part {part.Name} (Current: {part.QuantityInStock}, Min: {part.MinimumStock})");
            // TODO: Create alert log entry
        }

        // Check raw materials
        var lowMaterials = await db.RawMaterials
            .Where(rm => rm.QuantityInStock <= rm.MinimumStock)
            .ToListAsync(cancellationToken);

        foreach (var material in lowMaterials)
        {
            _logger.LogWarning($"Low stock alert: Raw material {material.Name} (Current: {material.QuantityInStock}, Min: {material.MinimumStock})");
            // TODO: Create alert log entry
        }

        _logger.LogInformation($"Inventory check complete: {lowSpareParts.Count} spare parts, {lowMaterials.Count} raw materials below minimum");
    }
}
