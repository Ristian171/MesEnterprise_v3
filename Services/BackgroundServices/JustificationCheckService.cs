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
            .FirstOrDefaultAsync(s => s.Key == SystemSettingKeys.RequireJustification, cancellationToken);

        if (requireJustificationSetting?.Value != "true")
        {
            return;
        }

        // Check recent production logs (last 24 hours) that haven't been checked yet
        var cutoffTime = DateTime.UtcNow.AddHours(-24);
        var recentLogs = await db.ProductionLogs
            .Include(l => l.Line)
            .Where(l => l.Timestamp > cutoffTime)
            .Where(l => !l.JustificationRequired || string.IsNullOrEmpty(l.JustificationReason))
            .ToListAsync(cancellationToken);

        int markedCount = 0;
        foreach (var log in recentLogs)
        {
            // Skip if already has justification
            if (!string.IsNullOrEmpty(log.JustificationReason))
                continue;

            // Check if line has OEE target set
            if (log.Line?.OeeTarget == null || log.Line.OeeTarget <= 0)
                continue;

            // Calculate OEE for this log
            double oee = log.TargetParts > 0 
                ? ((double)log.ActualParts / log.TargetParts) * 100.0 
                : 100.0;

            // If OEE is below target
            if (oee < log.Line.OeeTarget.Value)
            {
                // Check if there are sufficient justifications
                bool hasSufficientJustification = false;

                // Check for declared downtime
                if (log.DeclaredDowntimeMinutes.HasValue && log.DeclaredDowntimeMinutes.Value > 0)
                {
                    hasSufficientJustification = true;
                }

                // Check for scrap/NRFT with comments (defect allocations)
                if (!hasSufficientJustification)
                {
                    var hasDefectAllocations = await db.Set<MesEnterprise.Models.Production.ProductionLogDefect>()
                        .AnyAsync(pld => pld.ProductionLogId == log.Id, cancellationToken);
                    
                    if (hasDefectAllocations && (log.ScrapParts > 0 || log.NrftParts > 0))
                    {
                        hasSufficientJustification = true;
                    }
                }

                // If no sufficient justification, mark as requiring justification
                if (!hasSufficientJustification)
                {
                    log.JustificationRequired = true;
                    markedCount++;
                }
            }
        }

        if (markedCount > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Marked {markedCount} production logs as requiring justification");
        }

        var logsNeedingJustification = await db.ProductionLogs
            .Where(l => l.JustificationRequired && string.IsNullOrEmpty(l.JustificationReason))
            .Where(l => l.Timestamp > DateTime.UtcNow.AddDays(-7))
            .CountAsync(cancellationToken);

        _logger.LogInformation($"Total logs needing justification: {logsNeedingJustification}");
    }
}
