using MesEnterprise.Data;
using MesEnterprise.Models.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace MesEnterprise.Services.BackgroundServices;

public class JustificationCheckService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JustificationCheckService> _logger;
    private readonly TimeSpan _initialDelay;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(60);

    public JustificationCheckService(
        IServiceProvider serviceProvider,
        ILogger<JustificationCheckService> logger,
        IOptions<BackgroundServiceOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _initialDelay = options.Value.InitialDelay;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("JustificationCheckService starting");

        if (_initialDelay > TimeSpan.Zero)
        {
            await Task.Delay(_initialDelay, stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckJustificationsAsync(stoppingToken);
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("JustificationCheckService stopping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in JustificationCheckService");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task CheckJustificationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MesDbContext>();

        var requireJustificationSetting = await db.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == "Justification.Required", cancellationToken);

        if (requireJustificationSetting?.Value != "true")
        {
            return;
        }

        var thresholdSetting = await db.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == "Justification.ThresholdPercent", cancellationToken);

        var threshold = thresholdSetting != null ? int.Parse(thresholdSetting.Value!) : 85;

        var logsNeedingJustification = await db.ProductionLogs
            .Where(l => l.JustificationRequired && string.IsNullOrEmpty(l.JustificationReason))
            .Where(l => l.Timestamp > DateTime.UtcNow.AddDays(-7))
            .ToListAsync(cancellationToken);

        _logger.LogInformation($"Found {logsNeedingJustification.Count} logs needing justification");
    }
}
