using MesEnterprise.Data;
using MesEnterprise.Models.Alerts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MesEnterprise.Services.BackgroundServices;

public class AlertScannerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AlertScannerService> _logger;
    private readonly TimeSpan _initialDelay;
    private readonly TimeSpan _scanInterval = TimeSpan.FromMinutes(5);

    public AlertScannerService(
        IServiceProvider serviceProvider,
        ILogger<AlertScannerService> logger,
        IOptions<BackgroundServiceOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _initialDelay = options.Value.InitialDelay;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AlertScannerService starting");

        if (_initialDelay > TimeSpan.Zero)
        {
            await Task.Delay(_initialDelay, stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ScanForAlertsAsync(stoppingToken);
                await Task.Delay(_scanInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("AlertScannerService stopping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AlertScannerService");
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
        }
    }

    private async Task ScanForAlertsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MesDbContext>();

        var activeRules = await db.AlertRules
            .Where(r => r.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var rule in activeRules)
        {
            try
            {
                await EvaluateRuleAsync(db, rule, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error evaluating alert rule {rule.Id}");
            }
        }

        _logger.LogDebug($"Scanned {activeRules.Count} alert rules");
    }

    private async Task EvaluateRuleAsync(MesDbContext db, AlertRule rule, CancellationToken cancellationToken)
    {
        // TODO: Implement rule evaluation logic based on rule type
        // - ScrapConsecutiv: Check for consecutive scrap entries
        // - DowntimePesteLimita: Check for extended downtime
        // - Custom rules based on conditions
        
        await Task.CompletedTask;
    }
}
