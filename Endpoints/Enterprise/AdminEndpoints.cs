using MesEnterprise.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MesEnterprise.Endpoints.Enterprise;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization("AdminOnly");

        // Database backup (stub)
        group.MapPost("/backup", async (ILogger<Program> logger) =>
        {
            logger.LogInformation("Database backup initiated");
            // TODO: Implement pg_dump backup
            return Results.Ok(new { message = "Backup initiated (stub)", timestamp = DateTime.UtcNow });
        });

        // Database restore (stub)
        group.MapPost("/restore", async (ILogger<Program> logger, string backupFile) =>
        {
            logger.LogInformation($"Database restore initiated from {backupFile}");
            // TODO: Implement pg_restore
            return Results.Ok(new { message = "Restore initiated (stub)", file = backupFile });
        });

        // Database optimization
        group.MapPost("/optimize", async (MesDbContext db, ILogger<Program> logger) =>
        {
            logger.LogInformation("Database optimization initiated");
            
            // Execute VACUUM ANALYZE (PostgreSQL specific)
            try
            {
                await db.Database.ExecuteSqlRawAsync("VACUUM ANALYZE");
                logger.LogInformation("VACUUM ANALYZE completed");
                
                return Results.Ok(new { message = "Database optimized successfully" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during database optimization");
                return Results.Problem("Optimization failed: " + ex.Message);
            }
        });

        // Reindex tables
        group.MapPost("/reindex", async (MesDbContext db, ILogger<Program> logger) =>
        {
            logger.LogInformation("Database reindex initiated");
            
            try
            {
                await db.Database.ExecuteSqlRawAsync("REINDEX DATABASE mesdb");
                logger.LogInformation("REINDEX completed");
                
                return Results.Ok(new { message = "Database reindexed successfully" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during database reindex");
                return Results.Problem("Reindex failed: " + ex.Message);
            }
        });

        // System statistics
        group.MapGet("/stats", async (MesDbContext db) =>
        {
            var stats = new
            {
                totalUsers = await db.Users.CountAsync(),
                totalLines = await db.Lines.CountAsync(),
                totalProducts = await db.Products.CountAsync(),
                totalProductionLogs = await db.ProductionLogs.CountAsync(),
                totalInterventii = await db.InterventieTichete.CountAsync(),
                totalWorkOrders = await db.ProductionWorkOrders.CountAsync(),
                totalSpareParts = await db.SpareParts.CountAsync(),
                totalRawMaterials = await db.RawMaterials.CountAsync(),
                databaseSize = "N/A", // TODO: Query PostgreSQL for actual DB size
                timestamp = DateTime.UtcNow
            };

            return Results.Ok(stats);
        });

        // Clear old logs (cleanup)
        group.MapDelete("/logs/cleanup", async (MesDbContext db, int daysToKeep = 90) =>
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            
            var oldLogs = await db.ProductionLogs
                .Where(l => l.Timestamp < cutoffDate)
                .ToListAsync();

            db.ProductionLogs.RemoveRange(oldLogs);
            await db.SaveChangesAsync();

            return Results.Ok(new { message = $"Deleted {oldLogs.Count} old production logs" });
        });

        return app;
    }
}
