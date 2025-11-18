using MesEnterprise.Data;
using MesEnterprise.Models.Export;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Channels;

namespace MesEnterprise.Services.BackgroundServices;

public class ExportWorkerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExportWorkerService> _logger;
    private readonly TimeSpan _initialDelay;
    private readonly Channel<int> _exportQueue;

    public ExportWorkerService(
        IServiceProvider serviceProvider,
        ILogger<ExportWorkerService> logger,
        IOptions<BackgroundServiceOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _initialDelay = options.Value.InitialDelay;
        _exportQueue = Channel.CreateUnbounded<int>();
    }

    public async Task QueueExportJobAsync(int jobId)
    {
        await _exportQueue.Writer.WriteAsync(jobId);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExportWorkerService starting");

        if (_initialDelay > TimeSpan.Zero)
        {
            await Task.Delay(_initialDelay, stoppingToken);
        }

        await foreach (var jobId in _exportQueue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessExportJobAsync(jobId, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing export job {jobId}");
            }
        }
    }

    private async Task ProcessExportJobAsync(int jobId, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MesDbContext>();

        var job = await db.ExportJobs.FindAsync(new object[] { jobId }, cancellationToken);
        if (job == null)
        {
            _logger.LogWarning($"Export job {jobId} not found");
            return;
        }

        try
        {
            job.Status = "Processing";
            await db.SaveChangesAsync(cancellationToken);

            // TODO: Implement actual export logic
            // 1. Query data based on job.Parameters
            // 2. Format as CSV/XLSX/JSON based on job.Format
            // 3. Save to /exports folder
            // 4. Update job.FilePath

            await Task.Delay(2000, cancellationToken); // Simulate export

            job.Status = "Completed";
            job.CompletedAt = DateTime.UtcNow;
            job.FilePath = $"/exports/export_{jobId}.csv";
            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Export job {jobId} completed successfully");
        }
        catch (Exception ex)
        {
            job.Status = "Failed";
            await db.SaveChangesAsync(CancellationToken.None); // Use a new token as the original might be cancelled
            throw;
        }
    }
}
